const DamageType = {
    fire: { multiplier: [1,1,0.5] },
    melt: { multiplier: [1,2,0.5] },
    kinetic: { multiplier: [1,0.5,0] },
    blood: { multiplier: [2,0.5,0] },
    electric: { multiplier: [0.5,1,1] },
    magnetic: { multiplier: [0.5,1,2] },
    wither: { multiplier: [1,0,0.5] },
    decay: { multiplier: [2,0,0.5] },
    cold: { multiplier: [1,0.5,1] },
    frost: { multiplier: [1,1,1] }
}

const StatusEffect = {
    burn: class {
        static icon = assets.fire
        static key = 'burn'
    },
    bleed: class {
        static icon = assets.blood
        static key = 'bleed'
        description = 'Deals physical damage on hit'
        stack = 1
        posthit(unit, action, target, map){
            if(!unit || !action.skill) return
            map.queue.push(map => this.execute(target, map))
        }
        preturn(unit, map){ this.stack-- }
        execute(target, map){
            const enemy = target
            const delta = Unit.damage(null, { ...DamageType.kinetic, damage: 1 }, enemy, map)
            Overlay.log(`${this.constructor.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return effects.damage([enemy], 0)
        }
    },
    shock: class Effect {
        static icon = assets.shock
        static key = 'shock'
        description = 'Deals electric damage to neighbours on hit (conduit) <i>only transmit lightning?</i>'
        stack = 1
        posthit(unit, action, target, map){
            if(!unit || !action.skill) return
            map.queue.push(map => this.execute(target, map))
        }
        preturn(unit, map){ this.stack-- }
        execute(target, map){
            target = map.indexTile(map.units.indexOf(target), [])
            const hits = vec2.cardinal.map(offset => {
                const enemy = map.getUnit(target[0] + offset[0], target[1] + offset[1])
                if(!enemy) return
                const delta = Unit.damage(null, { damage: 1, ...DamageType.electric, }, enemy, map)
                Overlay.log(`${this.constructor.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
                return enemy
            }).filter(Boolean)
            return effects.damage(hits, 0)
        }
    },
    plague: class {
        static icon = assets.poison
        static key = 'plague'
    },
}

class Targeting {
    static flags = {
        none: 0,
        self: 1 << 0,
        ally: 1 << 1,
        enemy: 1 << 2,
        ground: 1 << 3,
        all: ~0 >>> 0
    }
    static single({
        flags, range, cardinal, forward, radial
    }){
        const tiles = [], size = Math.ceil(range)
        for(let c = -size; c <= size; c++) for(let r = -size; r <= size; r++){
            if(cardinal && c !== 0 && r !== 0) continue
            if(radial && Math.hypot(c, r) > range) continue
            tiles.push([c,r])
        }
        return function*(origin, map){
            const unit = map.getUnit(origin[0], origin[1])
            const bounds = map.getBounds(unit.agent)
            const step = map.front.column < bounds[0] ? -1 : 1
            for(let offset of tiles){
                const target = vec2.add(offset, origin, [])
                const tile = map.getTile(target[0], target[1])
                const entity = map.getUnit(target[0], target[1])
                if(tile == null) continue
                if(forward && Math.sign(target[0] - origin[0]) !== step) continue

                if((flags & Targeting.flags.self) == 0 && entity === unit) continue
                if((flags & Targeting.flags.ally) == 0 && entity !== unit && entity && entity.agent === unit.agent) continue
                if((flags & Targeting.flags.enemy) == 0 && entity && entity.agent !== unit.agent) continue
                if((flags & Targeting.flags.ground) == 0 && entity == null) continue

                if((flags & Targeting.flags.enemy) == 0){
                    if(tile.agent !== unit.agent) continue
                }else if((flags & Targeting.flags.ally) == 0){
                    if(tile.agent === unit.agent) continue
                }

                yield { tile: [map.tileIndex(target[0], target[1])], target }
            }
        }
    }
}

class Skill {
    static advance = {
        evaluate(agent, map){
            const bounds = map.getBounds(agent)
            if(bounds[3] < bounds[2] || bounds[1] < bounds[0]) return false
            const step = bounds[0] <= map.front.column ? 1 : -1
            const frontline = step > 0 ? map.front.column : map.front.column + 1

            if(step > 0 && (bounds[1] !== frontline || bounds[2] <= frontline + 1)) return false
            if(step < 0 && (bounds[0] !== frontline || bounds[3] >= frontline - 1)) return false
            if(frontline + step < 0 || frontline + step >= map.columns) return false

            const columns = Math.max(map.rows, bounds[1] - bounds[0])
            const units = [], remove = []
            for(let r = 0; r < map.rows; r++)
                for(let next = null, c = 0; c < columns; c++){
                    const unit = map.getUnit(frontline - step * c, r)
                    const moveable = next == null && unit && unit.agent === agent && unit.action >= 0 && unit.skills.some(skill => skill.key === 'move')
                    next = moveable ? null : unit
                    if(moveable) units.push([frontline - step * c, r])
                    if(c === columns - 1 && next != null && !unit.skills.some(skill => skill.key === 'move')) remove.push([frontline - step * c, r])
                    else if(c === columns - 1 && next != null) return false
                }
            return { units, step, remove }           
        },
        execute(agent, map){
            const { units, remove, step } = this.evaluate(agent, map)
            Overlay.log(`${agent.name} advances forward`)
            remove.forEach(target => map.getUnit(target[0], target[1]).integrity.forEach(item => item.value = 0))
            map.updateFormation(map.front.column + step)
            map.skip = agent
            return Promise.all(units.map(target => {
                const unit = map.getUnit(target[0], target[1])
                map.setUnit(target[0], target[1], null)
                map.setUnit(target[0] + step, target[1], unit)
                
                return effects.move(unit, map.getTile(target[0] + step, target[1]))
            }))
        }
    }
}

const skills = [{
    key: 'move', tags: [], description: 'Move one tile in cardinal direction.',
    query: Targeting.single({ flags: Targeting.flags.ground, range: 1, cardinal: true }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        Overlay.log(`${unit} ${this.key} (${target})`)
        unit.action--
        map.setUnit(origin[0], origin[1], null)
        map.setUnit(target[0], target[1], unit)
        return effects.move(unit, map.getTile(target[0], target[1]))
    }
},{
    key: 'swap', tags: ['spear','sekhmet'], description: 'Swap with adjacent neighbour restoring one action to it. <i>stun interaction? diagonals? any melee class?</i>',
    query: Targeting.single({ flags: Targeting.flags.ally, range: 1, cardinal: true }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        const ally = map.getUnit(target[0], target[1])
        Overlay.log(`${unit} used ${this.key} ${ally}`)
        map.setUnit(origin[0], origin[1], ally)
        map.setUnit(target[0], target[1], unit)
        unit.action--
        ally.action++

        return effects.move(unit, map.getTile(target[0], target[1]))
        .add(effects.move(ally, map.getTile(origin[0], origin[1])), 0)
    }
},{
    key: 'static dash', tags: ['shield','thoth'], description: 'Dash forward dealing electrical damage based on travelled distance <i>require unit? friendly fire?</i>',
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        const range = step > 0 ? map.front.column - origin[0] : origin[0] - map.front.column - 1
        const target = [origin[0], origin[1]]
        for(let i = 0; i <= range; i++)
            if(map.getUnit(target[0] += step, target[1])) break
        const distance = Math.abs(target[0] - origin[0])
        const damage = distance * Unit.calculateDamage(unit, this)
        yield { tile: [map.tileIndex(target[0], target[1])], target, step, damage }
    },
    execute(origin, { damage, target, step }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        map.setUnit(origin[0], origin[1], null)
        map.setUnit(target[0] - step, target[1], unit)

        const hits = [target].map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.electric, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.melee(unit, map.getTile(target[0], target[1]), { impact: 'lightning', reset: { x: map.getTile(target[0] - step, target[1]).x } })
        .add(effects.damage(hits, 0, 'electric'), 'hit')
    }
},{
    key: 'flame slash', tags: ['shield','ra'], description: 'Fire slash melee. +1x for each empty flank.',
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        const multiplier = 1 + (map.getUnit(origin[0], origin[1] + 1) == null) + (map.getUnit(origin[0], origin[1] - 1) == null)
        const damage = multiplier * Unit.calculateDamage(unit, this)

        const targets = [-1,0,1].map(dy => [origin[0] + step, origin[1] + dy]).filter(target => map.getTile(target[0], target[1]))
        yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), targets, damage, step }
    },
    execute(origin, { targets, damage, step }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.fire, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.melee(unit, map.getTile(origin[0] + step, origin[1]), {})
        .add(effects.damage(hits, 0, 'fire'), 'hit')
    }
},{
    key: 'shock strike', tags: ['shield','thoth'], description: 'forward magnetic melee costs 2 actions. cost 1 energy to stun</i>',
    query: Targeting.single({ flags: Targeting.flags.enemy, range: 1, forward: true, cardinal: true }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        const damage = Unit.calculateDamage(unit, this)
        unit.action-=2
        const stun = unit.integrity[2] > 0 && (unit.integrity[2]-=1, true)
        const hits = [target].map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            if(stun) enemy.action--
            const delta = Unit.damage(unit, { damage, ...DamageType.magnetic, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.melee(unit, map.getTile(target[0], target[1]), { effect: true })
        .add(effects.damage(hits, 0, 'electric'), 'hit')
    }
},{
    key: 'leech strike', tags: ['shield','osiris'], description: 'toxic melee. leech life',
    query: Targeting.single({ flags: Targeting.flags.enemy, range: 1, forward: true }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--

        const enemy = map.getUnit(target[0], target[1])
        const limit = unit.integrity[0].max - unit.integrity[0].value
        const damage = Unit.calculateDamage(unit, this)
        const delta = Unit.damage(unit, { damage, ...DamageType.wither, limit, skill: this }, enemy, map)
        Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
        const restore = -delta.reduce((a,b)=>a+b,0)
        unit.integrity[0].value = Math.min(unit.integrity[0].value + restore, unit.integrity[0].max)
        Overlay.log(`${unit} restore ${restore} health`)

        return effects.melee(unit, enemy, {})
        .add(effects.damage([enemy], 0), 'hit')
        .add(() => unit.update(), 'hit')
    }
},{
    key: 'parry strike', tags: ['shield','sekhmet'], description: 'physical retaliate on melee hit',
    query: Targeting.single({ flags: Targeting.flags.self, range: 0 }),
    execute(origin, option, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        Unit.addStatusEffect(unit, this, unit, new this.effect(), map)
        Overlay.log(`${unit} used ${this.key}`)
        return effects.apply(unit, {})
    },
    effect: class Effect {
        static icon = assets.counter
        static key = 'retaliate'
        constructor(){
            this.stack = 1
        }
        posthit(unit, action, target, map){
            if(!unit || !action.skill) return
            if(!action.skill.tags.includes('shield') && !action.skill.tags.includes('spear')) return //if action != melee tag
            map.queue.push(map => this.execute(target.tile, unit.tile, map))
        }
        preturn(unit, map){ this.stack-- }
        execute(origin, target, map){
            const unit = map.getUnit(origin[0], origin[1])
            const enemy = map.getUnit(target[0], target[1])
            const damage = Unit.calculateDamage(unit, this)
            const delta = Unit.damage(unit, { damage, ...DamageType.kinetic }, enemy, map)
            Overlay.log(`${unit} used ${this.constructor.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)

            return effects.melee(unit, enemy, {})
            .add(effects.damage([enemy], 0), 'hit')
        }
    }
},{
    key: 'arrow rain', tags: ['bow','sekhmet'], description: 'physical volley targets correspond to allied unit formation',
    *query(origin, map){
        const step = origin[0] <= map.front.column ? 1 : -1
        const targets = []
        for(let c = 0; c < map.rows; c++) for(let r = 0; r < map.rows; r++){
            const c0 = step > 0 ? map.front.column - map.rows + 1 + c : map.front.column + 1 + c
            const c1 = step > 0 ? map.front.column + 1 + c : map.front.column - map.rows + 1 + c
            if(map.getUnit(c0, r) != null && map.getTile(c1, r) != null) targets.push([ c1, r ])
        }
        yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), targets }
    },
    execute(origin, { targets }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const damage = Unit.calculateDamage(unit, this)
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.kinetic, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.ranged(unit, targets.map(target => map.getTile(target[0], target[1])), { })
        .add(effects.damage(hits, 0), 'hit')
    }
},{
    key: 'sharp arrow', tags: ['bow','sekhmet'], description: 'physical ranged bleed. extra physical on bleeding targets',
    query: Targeting.single({ flags: Targeting.flags.enemy, range: 5, forward: true }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const damage = Unit.calculateDamage(unit, this)
        const enemy = map.getUnit(target[0], target[1])
        const bleed = enemy.effects.some(effect => effect instanceof StatusEffect.bleed)
        const delta = Unit.damage(unit, { damage, ...(bleed ? DamageType.blood : DamageType.kinetic), skill: this }, enemy, map)
        Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
        if(!bleed) Unit.addStatusEffect(unit, this, enemy, new StatusEffect.bleed(), map)

        return effects.ranged(unit, [enemy], {})
        .add(effects.damage([enemy], 0, 'blood'), 'hit')
    }
},{
    key: 'detonate arrow', tags: ['bow','ra'], description: 'fire ranged. AOE on impact. <i>(require unit?, allow to hit own?)</i>',
    query: Targeting.single({ flags: Targeting.flags.enemy | Targeting.flags.ground, range: 4, forward: true }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const damage = Unit.calculateDamage(unit, this)
        const hits = [[0,0],[1,0],[0,1],[-1,0],[0,-1]].map(offset => {
            const enemy = map.getUnit(target[0] + offset[0], target[1] + offset[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.fire, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.ranged(unit, [map.getTile(target[0], target[1])], { impact: 'explosion' })
        .add(effects.damage(hits, 0, 'fire'), 'hit')
    }
},{
    key: 'melt arrow', tags: ['bow','ra'], description: 'melting piercing ranged straight line <i>friendly fire?</i>',
    range: 3,
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        const targets = Array(this.range).fill().map((_, i) => [origin[0] + step + step * i, origin[1]])
        .filter(target => map.getTile(target[0], target[1]))
        yield {
            tile: targets.map(target => map.tileIndex(target[0], target[1])),
            targets, damage: Unit.calculateDamage(unit, this)
        }
    },
    execute(origin, { damage, targets }, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        unit.action--
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.melt, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.ranged(unit, [{ x: unit.x + this.range * step * Tile.size, y: unit.y }], { trail: 'arrow_effect' })
        .add(effects.damage(hits, 0, 'fire'), 'hit')
    }
},{
    key: 'storm arrow', tags: ['bow','thoth'], description: 'forward electric ranged. shock',
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        for(let i = 0; i < map.rows; i++){
            const target = step > 0 ? [map.front.column + 1 + i, origin[1]] : [map.front.column - i, origin[1]]
            if(map.getUnit(target[0], target[1]) == null) continue
            yield { tile: [map.tileIndex(target[0], target[1])], target, damage: Unit.calculateDamage(unit, this) }
            break
        }
    },
    execute(origin, { target, damage }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const enemy = map.getUnit(target[0], target[1])
        const delta = Unit.damage(unit, { damage, ...DamageType.electric, skill: this }, enemy, map)
        Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)

        Unit.addStatusEffect(unit, this, enemy, new StatusEffect.shock(), map)

        return effects.ranged(unit, [enemy], { impact: 'lightning' })
        .add(effects.damage([enemy], 0, 'electric'), 'hit')
    }
},{
    key: 'fire wall', tags: ['sceptre','ra'], description: 'fire row', range: 3,
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        const targets = Array(map.rows).fill().map((_,i) => [ origin[0] + this.range * step, i ])
        yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), targets, damage: Unit.calculateDamage(unit, this) }
    },
    execute(origin, { targets, damage }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.fire, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.apply(unit, {})
        .add(effects.damage(hits, 0, 'fire'))
    }
},{
    key: 'lunge thrust', tags: ['spear','sekhmet'], description: 'forward piercing physical melee bleed', range: 2,
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        const targets = Array.range(this.range).map(i => [ origin[0] + step + i * step, origin[1] ])
        .filter(target => map.getTile(target[0], target[1]) && map.getTile(target[0], target[1]).agent !== unit.agent)
        if(!targets.some(target => map.getUnit(target[0], target[1]))) return
        if(targets.length) yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), targets, damage: Unit.calculateDamage(unit, this) }
    },
    execute(origin, { targets, damage }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.kinetic, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            Unit.addStatusEffect(unit, this, enemy, new StatusEffect.bleed(), map)
            return enemy
        }).filter(Boolean)

        return effects.melee(unit, hits[hits.length - 1], {  })
        .add(effects.damage(hits, 0, 'blood'), 'hit')
    }
},{
    key: 'conduit thrust', tags: ['spear','thoth'], description: 'single electric melee. +1x for adjacent unit with energy',
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        for(let i = 1; i <= 2; i++){
            const target = [ origin[0] + i * step, origin[1] ]
            if(!map.getTile(target[0], target[1]) || map.getTile(target[0], target[1]).agent === unit.agent) continue
            if(!map.getUnit(target[0], target[1])) continue
            const multiplier = 1 + vec2.cardinal.filter(offset => {
                const neighbour = map.getUnit(target[0] + offset[0], target[1] + offset[1])
                return neighbour && neighbour.integrity[2].value > 0
            }).length
            yield { tile: [map.tileIndex(target[0], target[1])], target, damage: multiplier * Unit.calculateDamage(unit, this) }
            break
        }
    },
    execute(origin, { damage, target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--

        const enemy = map.getUnit(target[0], target[1])
        const delta = Unit.damage(unit, { damage, ...DamageType.electric, skill: this }, enemy, map)
        Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)

        return effects.melee(unit, enemy, { impact: 'lightning' })
        .add(effects.damage([enemy], 0, 'electric'), 'hit')
    }
},{
    key: 'light thrust', tags: ['spear','ra'], description: 'fire piercing. +1 range for empty on same lane',
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1

        const range = 1 + Array.range(map.rows).map(c => step > 0 ? map.front.column - c : map.front.column + 1 + c)
        .map(c => [c, origin[1]]).filter(target => map.getUnit(target[0], target[1]) == null).length

        const targets = Array.range(range).map(c => [ origin[0] + step + c * step, origin[1] ])
        .filter(target => map.getTile(target[0], target[1]))
        
        yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), step: step * range, targets, damage: Unit.calculateDamage(unit, this) }
    },
    execute(origin, { damage, targets, step }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--

        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.fire, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.melee(unit, { x: unit.x + step * Tile.size, y: unit.y }, {})
        .add(effects.damage(hits, 0, 'fire'), 'hit')
    }
},{
    key: 'spawn obelisk', tags: ['sceptre','thoth'], description: 'create stationary unit. cost 2 action',
    query: Targeting.single({ flags: Targeting.flags.ground, range: 2 }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action -= 2

        Overlay.log(`${unit} used ${this.key} (${target})`)
        map.create(target[0], target[1], {
            agent: unit.agent, skills: [].map(key => skills.find(skill => skill.key === key)),
            slots: [Archetype.construct]
        }).action++

        return effects.apply(unit, {})
    }
}, {
    key: 'recharge', tags: ['sceptre','thoth'], description: 'restore energy to unit equal to number of adjacent units',
    query: Targeting.single({ flags: Targeting.flags.ally | Targeting.flags.self, range: 2 }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const ally = map.getUnit(target[0], target[1])

        const multiplier = vec2.cardinal.filter(offset => map.getUnit(target[0] + offset[0], target[1] + offset[1])).length
        Overlay.log(`${unit} used ${this.key} ${ally} restoring ${multiplier} ${Unit.integrity[2]}`)
        ally.integrity[2].value += multiplier

        return effects.apply(unit, {})
        .add(effects.apply(ally, {}), 0)
        .add(() => ally.update(), 0)
    }
}, {
    key: 'blood flow', tags: ['sceptre','osiris'], description: 'restore health to unit. costs health',
    query: Targeting.single({ flags: Targeting.flags.ally, range: 1 }),
    execute(origin, { target }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const ally = map.getUnit(target[0], target[1])

        const limit = ally.integrity[0].max - ally.integrity[0].value
        const restore = Math.max(0, Math.min(unit.integrity[0].value - 1, limit))
        Overlay.log(`${unit} used ${this.key} ${ally} restoring ${restore} ${Unit.integrity[0]}`)
        unit.integrity[0].value -= restore
        ally.integrity[0].value += restore

        return effects.apply(unit, {})
        .add(effects.apply(ally, {}), 0)
        .add(() => unit.update(), 0)
        .add(() => ally.update(), 0)
    }
}, {
    key: 'sun beams', tags: ['sceptre','ra'], description: 'fire at corresponding empty locations',
    *query(origin, map){
        const step = origin[0] <= map.front.column ? 1 : -1
        const targets = []
        for(let c = 0; c < map.rows; c++) for(let r = 0; r < map.rows; r++){
            const c0 = step > 0 ? map.front.column - map.rows + 1 + c : map.front.column + 1 + c
            const c1 = step > 0 ? map.front.column + 1 + c : map.front.column - map.rows + 1 + c
            if(map.getUnit(c0, r) == null && map.getTile(c1, r) != null) targets.push([ c1, r ])
        }
        yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), targets }
    },
    execute(origin, { targets }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const damage = Unit.calculateDamage(unit, this)
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.fire, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.apply(unit, { })
        .add(effects.damage(hits, 0, 'fire'), 'hit')
    }
}, {
    key: 'cascade strike', tags: ['shield','ra'], description: 'fire melee. hit unit behind if exists',
    *query(origin, map){
        const unit = map.getUnit(origin[0], origin[1])
        const step = origin[0] <= map.front.column ? 1 : -1
        const targets = []
        for(let c = 1; c <= map.rows; c++){
            if(map.getTile(origin[0] + c * step, origin[1]) == null || map.getUnit(origin[0] + c * step, origin[1]) == null) break
            targets.push([ origin[0] + c * step, origin[1] ])
        }
        yield { tile: targets.map(target => map.tileIndex(target[0], target[1])), targets, step, damage: Unit.calculateDamage(unit, this) }
    },
    execute(origin, { targets, step, damage }, map){
        const unit = map.getUnit(origin[0], origin[1])
        unit.action--
        const hits = targets.map(target => {
            const enemy = map.getUnit(target[0], target[1])
            if(!enemy) return
            const delta = Unit.damage(unit, { damage, ...DamageType.fire, skill: this }, enemy, map)
            Overlay.log(`${unit} used ${this.key} ${enemy} dealing ${delta.map((value, i) => `${value} ${Unit.integrity[i]}`).join(' ')}`)
            return enemy
        }).filter(Boolean)

        return effects.melee(unit, { x: unit.x + step * Tile.size, y: unit.y }, {})
        .add(effects.damage(hits, 0, 'fire'), 'hit')
    }
}]