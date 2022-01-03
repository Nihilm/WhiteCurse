class Tile extends PIXI.Graphics {
    static size = 128
    constructor(column, row){
        super()
        this.update()
        this.x = (this.column = column) * Tile.size
        this.y = (this.row = row) * Tile.size
        this.interactive = true
        this.on('pointerover', function(){
            this.hover = true
            this.update()
        }).on('pointerout', function(){
            this.hover = false
            this.update()
        }).on('pointerdown', function(){
            const tile = this.parent.tiles.indexOf(this)
            this.parent.emit('select', tile)
        })
    }
    get highlight(){ return this._highlight }
    set highlight(value){
        this._highlight = value
        this.update()
    }
    update(){
        this.clear()
        .lineStyle(1, 0xAAAAAA, 1, 0)
        .beginFill(0x777777, 0.2)
        .drawRect(-0.5 * Tile.size, -0.5 * Tile.size, Tile.size, Tile.size)
        .endFill()

        if(this.highlight) this.beginFill(0x77AA22, 1)
        .drawRect(-0.5 * Tile.size, -0.5 * Tile.size, Tile.size, Tile.size)
        .endFill()

        if(this.hover) this.beginFill(0xFFFFFF, 0.2)
        .drawRect(-0.5 * Tile.size, -0.5 * Tile.size, Tile.size, Tile.size)
        .endFill()
    }
}

class Battlefield extends PIXI.Container {
    static async play(levels){
        let formation = levels[0].units.filter(unit => unit.agent === PlayerAgent)
        for(let level of levels){
            level.units = formation.concat(level.units.filter(unit => unit.agent != PlayerAgent))
            formation = await Battlefield.instance.load(level)
            if(!formation) break
        }
    }
    queue = []
    constructor(){
        super()
        Battlefield.instance = this
        this.sortableChildren = true
        this.selection = new PIXI.Graphics().lineStyle(4, 0x77FFAA, 1, 0)
        .drawRect(-0.5 * Tile.size, -0.5 * Tile.size, Tile.size, Tile.size)
        .endFill()
        this.selection.animation = gsap.timeline({ repeat: -1 })
        .to(this.selection.scale, { duration: 0.5, x: 1.2, y: 1.2, ease: 'quad.out' }, 0)
        .to(this.selection.scale, { duration: 0.5, x: 1.0, y: 1.0, ease: 'quad.in' }, 0.5)

        this.leftFront = new PIXI.Graphics()
        this.rightFront = new PIXI.Graphics()
        this.effects = new PIXI.Container()
        this.effects.zIndex = 128
        this.addChild(this.effects)
    }
    clear(){
        this.tiles.forEach(tile => tile.destroy({ children: true }))
        this.units.forEach(unit => unit && unit.destroy({ children: true }))
        this.front.destroy()
    }
    async load({ key, columns, rows, front, units }){
        Overlay.log(`Enter ${columns}x${rows} room "${key}"`)
        this.columns = columns
        this.rows = rows
        this.tiles = Array(columns * rows)
        for(let c = 0; c < columns; c++) for(let r = 0; r < rows; r++)
            this.addChild(this.tiles[r + c * rows] = new Tile(c, r))

        this.units = Array(columns * rows).fill(null)
        units.forEach(unit => {
            const card = new UnitCard(unit)
            card.position.copyFrom(this.tiles[unit.tile])
            this.units[unit.tile] = card
            this.addChild(card)
        })

        this.front = new PIXI.Graphics().lineStyle(4, 0xFF0000, 1, 0.5)
        .moveTo(0, -0.5*Tile.size - 20)
        .lineTo(0, (rows - 0.5) * Tile.size + 20)
        this.addChild(this.front)
        this.front.x = (front + 0.5) * Tile.size

        this.position.set(-(front + 0.5) * Tile.size, (-0.5 * rows + 0.5) * Tile.size)
        this.updateFormation(front)

        for(let agents = [PlayerAgent, AIAgent]; true; agents.push(agents.shift())){
            for(let i = this.units.length - 1; i >= 0; i--)
                if(this.units[i] && this.units[i].agent === agents[0]) Unit.open(this.units[i])
            if(!(this.skip = this.skip === agents[0] ? null : this.skip))
            for await(let action of agents[0].execute(this)){
                await action(this)
                while(this.queue.length) await this.queue.shift()(this)                

                for(let i = this.units.length - 1; i >= 0; i--){
                    const unit = this.units[i]
                    if(!unit || unit.alive) continue
                    Overlay.log(`${unit} died`)
                    this.units[i] = null
                    await effects.death(unit)
                }
            }
            for(let i = this.units.length - 1; i >= 0; i--)
                if(this.units[i] && this.units[i].agent === agents[0]) Unit.seal(this.units[i], this)
            while(this.queue.length) await this.queue.shift()(this)

            const line = this.getBounds(PlayerAgent)
            if(line[0] <= line[1] && line[2] <= line[3]) continue
            else if(line[0] > line[1]){
                Overlay.log(`${(agents[0] === PlayerAgent ? agents[0] : agents[1]).name} was eliminated`)
                await Overlay.instance.showMessage(`Defeat`).add(() => this.clear(), 'transition')
                return null
            }else{
                Overlay.log(`${(agents[1] === PlayerAgent ? agents[0] : agents[1]).name} was eliminated`)
                //TODO allow to edit formation before exiting?
                const formation = this.units.filter(unit => unit && unit.agent === PlayerAgent).map(unit => unit.toJSON())
                formation.forEach(unit => unit.tile -= line[0] * this.rows)
                await Overlay.instance.showMessage('Victory').add(() => this.clear(), 'transition')
                return formation
            }
        }
    }
    create(column, row, options){
        const unit = new UnitCard(options)
        this.addChild(unit)
        this.setUnit(column, row, unit)
        unit.position.copyFrom(this.getTile(column, row))
        return unit
    }
    updateFormation(frontline){
        if(this.front.column === frontline) return
        this.front.column = frontline
        // const bounds = this.getBounds(PlayerAgent)
        // const step = frontline < bounds[0] ? -1 : 1
        for(let c = 0; c < this.columns; c++) for(let r = 0; r < this.rows; r++){
            const tile = this.tiles[r + c * this.rows]
            if(c >= frontline - this.rows && c <= frontline) tile.agent = PlayerAgent
            else if(c >= frontline + 1 && c < frontline + 1 + this.rows) tile.agent = AIAgent
            else tile.agent = null
        }
        return gsap.timeline()
        .to(this.front, { duration: 0.2, x: (frontline + 0.5) * Tile.size, ease: 'quad.out' }, 0)
        .to(this, { duration: 0.4, x: -(frontline + 0.5) * Tile.size, ease: 'quad.inOut' }, 0)
    }
    tileIndex(column, row){ return row + column * this.rows }
    indexTile(index, out = []){
        out[0] = index / this.rows | 0
        out[1] = index % this.rows
        return out
    }
    setUnit(column, row, unit){
        this.units[row + column * this.rows] = unit
    }
    getUnit(column, row){
        if(column < 0 || row < 0 || column >= this.columns || row >= this.rows) return null
        return this.units[row + column * this.rows]
    }
    getTile(column, row){
        if(column < 0 || row < 0 || column >= this.columns || row >= this.rows) return null
        return this.tiles[row + column * this.rows]
    }
    getBounds(agent){
        const bounds = [Infinity, -Infinity, Infinity, -Infinity]
        for(let c = 0; c < this.columns; c++) for(let r = 0; r < this.rows; r++){
            const unit = this.units[r + c * this.rows]
            if(!unit) continue
            const offset = unit.agent === agent ? 0 : 2
            bounds[offset + 0] = Math.min(bounds[offset + 0], c)
            bounds[offset + 1] = Math.max(bounds[offset + 1], c)
        }
        return bounds
    }
}