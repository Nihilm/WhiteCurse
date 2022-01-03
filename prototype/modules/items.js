const Archetype = {
    archer: { key: 'archer', integrity: [2,0,0], tags: ['bow'], slot: 'passive', icon: assets.bow },
    priest: { key: 'priest', integrity: [2,0,0], tags: ['sceptre'], slot: 'passive', icon: assets.sceptre },
    shieldbearer: { key: 'shieldbearer', integrity: [4,0,0], tags: ['shield'], slot: 'passive', icon: assets.sword_shield },
    spearman: { key: 'spearman', integrity: [3,0,0], tags: ['spear'], slot: 'passive', icon: assets.spear },
    construct: { key: 'construct', integrity: [0,2,0], tags: [], slot: 'passive', icon: assets.gear },
    apparition: { key: 'apparition', integrity: [0,0,3], tags: [], slot: 'passive', icon: assets.ectoplasm }
}

const Deity = {
    Ra: [
        { key: 'fire', slot: 'affinity' },
        { key: 'ice', slot: 'affinity' }
    ],
    Osiris: [
        { key: 'life', slot: 'affinity' },
        { key: 'death', slot: 'affinity' }
    ],
    Sekhmet: [
        { key: 'offence', slot: 'affinity' },
        { key: 'defence', slot: 'affinity' }
    ],
    Thoth: [
        { key: 'technology', slot: 'affinity' },
        { key: 'miracles', slot: 'affinity' }
    ],
    Seth: [
        { key: 'order', slot: 'affinity' },
        { key: 'chaos', slot: 'affinity' }
    ]
}

const items = {
    sceptre: [
        { key: 'sceptre', tags: ['sceptre'], damage: 1, slot: 'weapon' }
    ],
    bow: [
        { key: 'composite bow', tags: ['bow','ranged'], damage: 1, slot: 'weapon' }
    ],
    spear: [
        { key: 'spear', tags: ['spear','melee'], damage: 2, slot: 'weapon' }
    ],
    shield: [
        { key: 'khopesh', tags: ['sword_shield','melee'], damage: 1, slot: 'weapon' },
        { key: 'axe', tags: ['sword_shield','melee'], damage: 4, slot: 'weapon' }
    ],
    armor: [
        { key: 'garment', integrity: [2,0,0] },
        { key: 'tunic', integrity: [1,0,2] },
        { key: 'leather armor', integrity: [2,2,0] }
    ]
}