class Unit {
    static integrity = ['health', 'armor', 'energy']
    static damage(source, action, target, context){
        const delta = Array(target.integrity.length).fill(0)
        let remaining = action.damage, total = 0
        for(let i = delta.length - 1; remaining && i >= 0; i--){
            const multiplier = action.multiplier[i]
            const prev = target.integrity[i].value
            if(!multiplier || !prev) continue
            const value = prev - Math.sign(remaining) * Math.floor(Math.abs(remaining * multiplier))
            const next = Math.max(0, Math.min(target.integrity[i].max, value))
            const overflow = value - next
            remaining = -Math.sign(overflow) * Math.floor(Math.abs(overflow / multiplier))
            target.integrity[i].value = next
            delta[i] = next - prev
            total += delta[i]
        }

        if(action.limit < -total) for(let i = 0, limit = -action.limit - total; i < delta.length && limit; i++){
            const overflow = Math.min(Math.abs(delta[i]), limit)
            target.integrity[i].value -= overflow * Math.sign(delta[i])
            delta[i] -= overflow * Math.sign(delta[i])
            limit -= overflow
        }

        for(let i = target.effects.length - 1; i >= 0; i--){
            if(!target.effects[i].posthit) continue
            target.effects[i].posthit(source, action, target, context)
            if(!target.effects[i].stack) target.effects.splice(i, 1)
        }
    
        return delta
    }
    static addStatusEffect(unit, skill, target, effect, context){
        const index = target.effects.findIndex(prev => prev instanceof effect.constructor)
        if(index != -1){
            effect.stack += target.effects[index].stack
            target.effects.splice(index, 1)
        }
        target.effects.push(effect)
    }
    static open(unit, map){
        unit.action++
        for(let i = unit.effects.length - 1; i >= 0; i--){
            if(!unit.effects[i].preturn) continue
            unit.effects[i].preturn(unit, map)
            if(!unit.effects[i].stack) unit.effects.splice(i, 1)
        }
        unit.update()
    }
    static seal(unit, map){
        for(let i = unit.effects.length - 1; i >= 0; i--){
            if(!unit.effects[i].postturn) continue
            unit.effects[i].postturn(unit, map)
            if(!unit.effects[i].stack) unit.effects.splice(i, 1)
        }
        unit.update()
    }
    static calculateDamage({ slots }, skill){
        let value = 0
        for(let item of slots) if(item.damage) value += item.damage
        return value
    }
    static calculateIntegrity({ slots }){
        const capacity = Array(Unit.integrity.length).fill(0)
        for(let i = 0; i < capacity.length; i++)
            for(let item of slots) if(item.integrity) capacity[i] += item.integrity[i]
        return capacity
    }
}

class UnitCard extends PIXI.Container {
    constructor(options){
        super()
        this.zIndex = 0
        this.skills = options.skills
        this.slots = options.slots
        this.agent = options.agent
        this.effects = []
        this.card = new PIXI.Graphics().beginFill(options.agent === PlayerAgent ? 0x77AAFF : 0xFF77AA)
        .drawRect(-0.4 * Tile.size, -0.4 * Tile.size, 0.8*Tile.size, 0.8*Tile.size).endFill()
        this.addChild(this.card)

        this.item = new PIXI.Sprite(this.slots.find(item => item.icon).icon)
        this.item.scale.set(Tile.size / 200)
        this.item.anchor.set(0.5)
        this.addChild(this.item)

        this.integrity = Unit.calculateIntegrity(this).map((max, i) => {
            const key = Unit.integrity[i]
            const icon = new PIXI.Sprite(assets[key])
            icon.anchor.set(0.5)
            icon.scale.set(0.1)
            icon.position.set(i * 24, -48)
            this.addChild(icon)
            const amount = new PIXI.Text('', new PIXI.TextStyle({
                fontFamily: 'Arial', fontSize: 20, fontWeight: 'bold', fill: ['#ffffff']
            }))
            this.addChild(amount)
            amount.anchor.set(0.5)
            amount.position.copyFrom(icon.position)
            return { key, icon, amount, value: max, max }
        })

        this.ailments = Array(5).fill().map((_,i) => {
            const icon = new PIXI.Sprite(PIXI.Texture.EMPTY)
            icon.anchor.set(0.5)
            icon.scale.set(0.3)
            icon.position.set(i * 24, -48 + 24)
            this.addChild(icon)
            const amount = new PIXI.Text('', new PIXI.TextStyle({
                fontFamily: 'Arial', fontSize: 20, fontWeight: 'bold', fill: ['#ffffff']
            }))
            this.addChild(amount)
            amount.anchor.set(0.5)
            amount.position.copyFrom(icon.position)
            return { index: -1, icon, amount }
        })

        this.update()
    }
    get action(){ return this._action || 0 }
    set action(value){ this._action = Math.min(1, value); this.update() }
    update(){
        this.integrity.forEach(item => {
            item.amount.text = item.value
            item.icon.tint = item.amount.tint = item.value ? 0xFFFFFF : 0x777777
        })
        if(this.action == 0) this.card.tint = 0x777777
        else if(this.action > 0) this.card.tint = 0xFFFFFF
        else this.card.tint = 0x440000

        let index = 0
        for(let i = 0; i < this.effects.length; i++){
            if(!this.effects[i].constructor.icon) continue
            this.ailments[index].icon.texture = this.effects[i].constructor.icon
            this.ailments[index].amount.text = this.effects[i].stack
            index++
        }
        for(; index < this.ailments.length; index++){
            this.ailments[index].icon.texture = PIXI.Texture.EMPTY
            this.ailments[index].amount.text = ''
        }
    }
    get tile(){
        const index = this.parent.units.indexOf(this)
        return this.parent.indexTile(index)
    }
    toString(){
        return `(${this.tile})`
    }
    toJSON(){
        return { tile: this.parent.units.indexOf(this), agent: this.agent, skills: this.skills, slots: this.slots }
    }
    get alive(){
        for(let i = 0; i < this.integrity.length; i++)
            if(this.integrity[i].max) return this.integrity[i].value > 0
    }
}