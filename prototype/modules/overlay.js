class Overlay extends PIXI.Container {
    static log(message){
        const line = document.createElement('div')
        line.innerHTML = message
        Overlay.instance.console.prepend(line)
    }
    static showDescription(unit){
        Overlay.instance.selection.innerHTML = !unit ? '' :
`<div>
<h3>Selected ${unit} (${unit.agent.name})</h3>
<p>action ${unit.action}/1 ${unit.integrity.map(stat => `${stat.key} ${stat.value}/${stat.max}`).join(' ')}</p>
<p>${unit.effects.map(effect => `${effect.constructor.key || effect.key} ${effect.stack}`).join(' ')}</p>
<h4>Skills</h4>
<ul>${unit.skills.map(skill => (
`<li><b>${skill.key}</b> ${skill.description}<br>${skill.tags.join(' ')}</li>`
)).join('\n')}</ul>
<h4>Equipment & Traits</h4>
<ul>${unit.slots.map(function(item){
    return `<li>${item.key} ${
        item.integrity ? item.integrity.map((value, i) => value && `<em>+${value}</em> ${Unit.integrity[i]}`).filter(Boolean).join(' ') : ''
    }${
        item.damage ? `<em>+${item.damage}</em> damage` : ''
    }</li>`
}).join('\n')}</ul>
</div>`
    }
    static slotSize = 74
    constructor(){
        super()
        Overlay.instance = this
        document.body.appendChild(this.selection = document.createElement('div'))
        document.body.appendChild(this.console = document.createElement('div'))

        this.pass = new PIXI.Text('END TURN', new PIXI.TextStyle({
            fontFamily: 'Arial', fontSize: 36, fontWeight: 'bold', fill: ['#ffffff'], textBaseline: 'bottom'
        }))
        this.pass.anchor.set(0.5)
        this.pass.y = 250
        this.pass.interactive = true
        this.pass.buttonMode = true
        this.pass.on('pointerdown', () => this.emit('pass'))
        this.pass.visible = false
        this.addChild(this.pass)

        this.advance = new PIXI.Text('ADVANCE', new PIXI.TextStyle({
            fontFamily: 'Arial', fontSize: 36, fontWeight: 'bold', fill: ['#ffffff'], textBaseline: 'bottom'
        }))
        this.advance.anchor.set(0.5)
        this.advance.y = 250
        this.advance.x = 250
        this.advance.interactive = true
        this.advance.buttonMode = true
        this.advance.on('pointerdown', () => this.emit('advance'))
        this.advance.visible = false
        this.addChild(this.advance)


        this.skills = Array(8).fill(null).map((_,i) => {
            const slot = new PIXI.Graphics().lineStyle(1, 0x777777, 1, 0).beginFill(0x222222, 1).drawRect(0, 0, Overlay.slotSize, Overlay.slotSize).endFill()
            slot.position.set(i * Overlay.slotSize - 400 + 36/2, -300 + 36/2)
            slot.interactive = true
            slot.on('pointerover', function(){
                this.tint = 0x77FF77
            }).on('pointerout', function(){
                this.tint = 0xFFFFFF
            }).on('pointerdown', () => this.emit('select', i))
            this.addChild(slot)
            slot.visible = false
            const title = new PIXI.Text('', new PIXI.TextStyle({
                fontFamily: 'Arial', fontSize: 20, fontWeight: 'bold', fill: ['#ffffff'], textBaseline: 'bottom',
                wordWrap: true, align: 'center', wordWrapWidth: Overlay.slotSize
            }))
            title.position.copyFrom(slot.position)
            this.addChild(title)
            return { slot, title }
        })
    }
    selectUnit(unit){
        const length = unit ? unit.skills.length : 0
        for(let i = 0; i < length; i++){
            this.skills[i].slot.visible = true
            this.skills[i].title.text = unit.skills[i].key
        }
        for(let i = length; i < this.skills.length; i++){
            this.skills[i].slot.visible = false
            this.skills[i].title.text = ''
        }
    }
    showMessage(text){
        const message = new PIXI.Text(text, new PIXI.TextStyle({
            fontFamily: 'Arial', fontSize: 48, fontWeight: 'bold', fill: ['#ffffff'], align: 'center', strokeThickness: 4
        }))
        message.anchor.set(0.5)
        const fade = new PIXI.Sprite(PIXI.Texture.WHITE)
        fade.tint = 0x08080E
        fade.scale.set(1024)
        fade.anchor.set(0.5)

        this.addChild(fade)
        this.addChild(message)

        return gsap.timeline()
        .fromTo(fade, { alpha: 0 }, { duration: 0.8, alpha: 1, ease: 'quad.out' }, 0)
        .to(fade, { duration: 0.4, alpha: 0, ease: 'quad.in' }, 0.8)
        .add(() => fade.destroy(), 1.2)
        
        .fromTo(message.scale, { x: 0, y: 0 }, { duration: 0.8, x: 1, y: 1, ease: 'elastic.out(1, 0.5)' }, 0)
        .fromTo(message, { alpha: 1 }, { duration: 0.4, alpha: 0, ease: 'quad.in' }, 0.8)
        .add(() => message.destroy(), 1.2)

        .addLabel('transition', 0.8)
    }
}