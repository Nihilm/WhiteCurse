const app = new PIXI.Application({ width: 800, height: 600 })
document.body.appendChild(app.view)
app.stage.position.set(app.renderer.width * 0.5, app.renderer.height * 0.5)

app.stage.addChild(new Battlefield())
app.stage.addChild(new Overlay())
// app.ticker.add(delta => Battlefield.instance.update(delta))

const inhabitants = [{
    key: 'brigand', agent: AIAgent, skills: ['move', 'cascade strike', 'leech strike'].map(key => skills.find(skill => skill.key === key)),
    slots: [Archetype.shieldbearer, items.shield[1], items.armor[2]]
},{
    key: 'brigand', agent: AIAgent, skills: ['move', 'light thrust', 'lunge thrust'].map(key => skills.find(skill => skill.key === key)),
    slots: [Archetype.spearman, items.spear[0], items.armor[2]]
},{
    key: 'brigand', agent: AIAgent, skills: ['move', 'detonate arrow', 'sharp arrow'].map(key => skills.find(skill => skill.key === key)),
    slots: [Archetype.archer, items.bow[0], items.armor[0]]
}]

function generateLevel(key, length, level){
    const quantity = [1,1,1,1,2,2,2,3,3]
    const rows = 3, units = []
    for(let c = 3; c < length; c++){
        const list = Array.range(rows).shuffle().slice(0, quantity[Math.random() * quantity.length | 0])
        for(let r of list){
            const unit = inhabitants[Math.random() * inhabitants.length | 0]
            units.push({ tile: r + c * rows, ...unit })
        }   
    }
    return { key, columns: length, rows, front: 2, units }
}

Battlefield.play([{
    key: 'Stage 1', columns: 6, rows: 3, front: 2, units: [{
        tile: 0, agent: PlayerAgent, skills: skills.filter(skill => !skill.tags.length || skill.tags.includes('bow')),
        slots: [Archetype.archer, items.bow[0], items.armor[0]]
    },{
        tile: 4, agent: PlayerAgent, skills: skills.filter(skill => !skill.tags.length || skill.tags.includes('spear')),
        slots: [Archetype.spearman, items.spear[0], items.armor[2]]
    },{
        tile: 5, agent: PlayerAgent, skills: skills.filter(skill => !skill.tags.length || skill.tags.includes('sceptre')),
        slots: [Archetype.priest, items.sceptre[0], items.armor[1]]
    },{
        tile: 7, agent: PlayerAgent, skills: skills.filter(skill => !skill.tags.length || skill.tags.includes('shield')),
        slots: [Archetype.shieldbearer, items.shield[0], items.armor[0]]
    },{
        tile: 10, ...inhabitants[0]
    },{
        tile: 12, ...inhabitants[1]
    }]
}, generateLevel('Stage 2', 9, 0)])