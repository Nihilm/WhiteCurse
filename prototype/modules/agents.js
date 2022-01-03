class AIAgent {
    static async *execute(map){
        Overlay.log(`<b>AI</b> turn.`)
        while(true){
            const frontline = map.front.column + 1
            for(let c = frontline + 1; c < map.columns; c++){
                const collapse = Array.range(map.rows).every(r => map.getUnit(c - 1, r) === null)
                if(!collapse) continue
                for(let i = 0; i < map.units.length; i++){
                    const unit = map.units[i]
                    if(!unit || unit.agent !== this || unit.action <= 0) continue
                    const origin = unit.tile
                    const move = unit.skills.find(skill => skill.key === 'move')
                    if(!move || origin[0] < c) continue
                    const option = Array.from(move.query(origin, map)).find(option => option.target[0] === origin[0] - 1)
                    if(option) yield context => move.execute(origin, option, context)
                }
                break
            }

            const remaining = map.units.filter((unit, i) => {
                if(map.tiles[i].agent !== this) return false
                return unit && unit.agent === this && unit.action > 0
            })
            for(let unit of remaining){
                const origin = map.indexTile(map.units.indexOf(unit), [])
                const skills = Array.range(unit.skills.length)
                for(let i = skills.length - 1; i >= 0; i--){
                    const skill = unit.skills[skills[i]]
                    const candidates = Array.from(skill.query(origin, map))
                    if(!candidates.length) continue
                    const option = candidates[Math.random() * candidates.length | 0]
                    yield context => skill.execute(origin, option, context)
                    break
                }
            }
            if(Skill.advance.evaluate(this, map)) yield context => Skill.advance.execute(this, context)
            break
        }
    }
}

class PlayerAgent {
    static unit = null
    static skill = null
    static map = null
    static async *execute(map){
        Overlay.log(`<b>Player</b> turn.`)

        this.map = map.on('select', this.selectTile)
        Overlay.instance.on('select', this.selectSlot)
        Overlay.instance.on('pass', this.pass)
        Overlay.instance.on('advance', this.advance)
        Overlay.instance.pass.visible = true
        
        while(true){
            Overlay.instance.advance.visible = Skill.advance.evaluate(this, map)
            const action = await new Promise(resolve => map.once('action', resolve))
            if(action === 'advance'){
                yield context => Skill.advance.execute(this, context)
                break
            }else if(action) yield action
            else break
        }
        //TODO deselect - enter view mode
        this.selectTile(-1)

        map.off('select', this.selectTile)
        Overlay.instance.off('select', this.selectSlot)
        Overlay.instance.off('pass', this.pass)
        Overlay.instance.off('advance', this.advance)
        Overlay.instance.pass.visible = false
        Overlay.instance.advance.visible = false
    }
    static advance = () => this.map.emit('action', 'advance')
    static pass = () => this.map.emit('action', null)
    static selectTile = tile => {
        if(this.skill && this.map.tiles[tile] && this.map.tiles[tile].highlight){
            const action = { origin: this.unit.tile, option: this.map.tiles[tile].option, skill: this.skill }
            this.selectSlot(-1)
            this.map.emit('action', context => action.skill.execute(action.origin, action.option, context))
            tile = -1
        }else{
            this.selectSlot(-1)
        }

        deselect: if(this.unit){
            Overlay.showDescription(null)
            Overlay.instance.selectUnit(null)
            this.map.removeChild(this.map.selection)
        }
        select: if(this.unit = this.map.units[tile]){
            this.map.addChild(this.map.selection)
            this.map.selection.position.copyFrom(this.unit.position)
            if(this.unit.action > 0 && this === this.unit.agent)
                Overlay.instance.selectUnit(this.unit)
            Overlay.showDescription(this.unit)
        }
    }
    static selectSlot = slot => {
        deselect: if(this.skill){
            this.map.tiles.forEach(tile => tile.option = tile.highlight = false)
            if(this.unit && this.skill === this.unit.skills[slot]){
                this.skill = null
                return
            }
            this.skill = null
        }
        if(!this.unit || !this.unit.skills[slot]) return
        this.skill = this.unit.skills[slot]
        const tile = this.map.units.indexOf(this.unit)
        for(let option of this.skill.query(this.map.indexTile(tile, []), this.map))
            for(let index of option.tile){
                this.map.tiles[index].highlight = true
                this.map.tiles[index].option = option
            }
    }
}