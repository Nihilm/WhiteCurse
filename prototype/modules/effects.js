const particles = {
    fire: target => new PIXI.particles.Emitter(target.parent.effects, {
        lifetime: { min: 0.5, max: 1 }, pos: { x: target.x, y: target.y },
        frequency: 0.001, spawnChance: 1, particlesPerWave: 4, emitterLifetime: 0.01, maxParticles: 128, addAtBack: false,
        ease: [
            {s: 0,cp: 0.329,e: 0.548},
            {s: 0.548,cp: 0.767,e: 0.876},
            {s: 0.876,cp: 0.985,e: 1}
        ],
        behaviors: [
            { type: 'blendMode', config: { blendMode: 'add' } },
            { type: 'alpha', config: { alpha: { list: [{ time: 0, value: 1 }, { time: 1, value: 0 }] } } },
            { type: 'moveSpeed', config: { speed: { list: [{ time: 0, value: 450 }, { time: 1, value: 0 }], isStepped: false } } },
            { type: 'scale', config: { scale: { list: [{ time: 0, value: 2 }, { time: 1, value: 0.1 }] }, minMult: 0.1 } },
            { type: 'color', config: { color: { list: [{ time: 0, value: '#ffffff' }, { time: 1, value: '#ff0000' }] } } },
            { type: 'spawnPoint', config: {} },
            { type: 'rotationStatic', config: { min: 0, max: 360 } },
            { type: 'textureSingle', config: { texture: assets.particle } }
        ]
    }).playOnceAndDestroy(function(){}),
    blood: target => new PIXI.particles.Emitter(target.parent.effects, {
        lifetime: { min: 0.2, max: 0.6 }, pos: { x: target.x, y: target.y },
        frequency: 0.002, spawnChance: 1, particlesPerWave: 4, emitterLifetime: 0.02, maxParticles: 128, addAtBack: true,
        behaviors: [
            { type: 'scale', config: { scale: { list: [{time:0,value:0},{time:0.25,value:1},{time:1,value:0}], isStepped: false }, minMult: 0.2 } },
            { type: 'rotation', config: { minStart: 0, maxStart: 360, minSpeed: -120, maxSpeed: 120, accel: 0 } },
            { type: 'textureSingle', config: { texture: assets.blood_particle } },
            { type: 'spawnBurst', config: { distance: 0, spacing: 0, start: 0 } },
            { type: 'moveSpeedStatic', config: { min: 100, max: 150 } }
        ]
    }).playOnceAndDestroy(function(){}),
    electric: target => new PIXI.particles.Emitter(target.parent.effects, {
        lifetime: { min: 0.1, max: 0.2 }, pos: { x: target.x, y: target.y },
        frequency: 0.01, spawnChance: 1, particlesPerWave: 4, emitterLifetime: 0.2, maxParticles: 128, addAtBack: false,
        behaviors: [
            { type: 'blendMode', config: { blendMode: 'add' } },
            { type: 'alpha', config: { alpha: { list: [{value: 0, time: 0}, {value: 1, time: 0.25}, {value: 0, time: 1}] } } },
            { type: 'scaleStatic', config: { min: 0.5, max: 2 } },
            { type: 'rotationStatic', config: { min: 0, max: 360 } },
            { type: 'spawnShape', config: { type: 'torus', data: { x: 0, y: 0, radius: 128, innerRadius: 0, affectRotation: false } } },
            { type: 'textureRandom', config: { textures: [assets.lightning_particle] } },
        ]
    }).playOnceAndDestroy(function(){}),
}

const effects = {
    death: unit => {
        return gsap.timeline()
        .to(unit.scale, { duration: 1, x: 0, y: 0, ease: 'cubic.in' })
        .add(() => unit.destroy({ children: true }))
    },
    damage: (units, delay = 0, impact) => {
        if(!units.length) return gsap.timeline()
        return gsap.timeline()
        .fromTo(units.map(item=>item.scale), { x: 0.5, y: 1.2 }, { duration: 0.2, x: 1, y: 1, ease: 'quad.out', immediateRender: false }, delay)
        .add(() => impact && units.forEach(particles[impact]), delay)
        .add(() => units.forEach(item=>item.update()), delay)
    },
    move: (unit, target) => {
        return gsap.timeline()
        .to(unit, { duration: 0.2, x: target.x, y: target.y, ease: 'quad.inOut' })
    },
    apply: (unit, options) => {
        const timeline = gsap.timeline()
        timeline.to(unit.scale, { duration: 0.3, x: 0.5, y: 0.5, ease: 'quad.in' }, 0)
        .to(unit.scale, { duration: 0.2, x: 1, y: 1, ease: 'quad.out' }, 0.3)
        .add(() => unit.update(), 0.3)

        const glow = new PIXI.Sprite(PIXI.Texture.WHITE)
        glow.anchor.set(0.5)
        glow.scale.set(Tile.size / glow.width)
        glow.blendMode = PIXI.BLEND_MODES.ADD
        unit.addChild(glow)
        timeline.fromTo(glow, { alpha: 0 }, { duration: 0.3, alpha: 1, ease: 'quad.out' }, 0)
        .to(glow, { duration: 0.2, alpha: 0, ease: 'quad.in' }, 0.3)
        .add(() => glow.destroy(), 0.5)

        return timeline
    },
    melee: (unit, target, options) => {
        const timeline = gsap.timeline()

        timeline
        .set(unit, { zIndex: 10 }, 0)
        .to(unit, { duration: 0.2, x: target.x, y: target.y, ease: 'back.in(1.5)' }, 0)
        .to(unit, { duration: 0.3, x: unit.x, y: unit.y, ease: 'quad.out', ...options.reset }, 0.2)
        .set(unit, { zIndex: 0 }, 0.5)

        if(options.impact){
            const effect = new PIXI.Sprite(assets[options.impact])
            effect.zIndex = 12
            effect.anchor.set(0.5)
            effect.blendMode = PIXI.BLEND_MODES.ADD
            unit.parent.addChild(effect)
            timeline.fromTo(effect, { x: target.x, y: target.y, alpha: 1 }, { duration: 0.4, alpha: 0, ease: 'quad.in' }, 0.2)
            .fromTo(effect.scale, { x: 0, y: 0 }, { duration: 0.4, x: 1.6, y: 1.6, ease: 'quad.out' }, 0.2)
            .add(() => effect.destroy(), 0.6)
        }

        timeline.addLabel('hit', 0.2)
        return timeline
    },
    ranged: (unit, target, options) => {
        const timeline = gsap.timeline()

        timeline.set(unit, { zIndex: 10 }, 0)
        .fromTo(unit, { rotation: 2*Math.PI }, { duration: 0.5, rotation: 0, ease: 'quad.out' }, 0)
        .set(unit, { zIndex: 0 }, 0.5)

        target.forEach(target => {
            const arrow = new PIXI.Sprite(assets.arrow)
            arrow.anchor.set(0.5, 0.5)
            unit.parent.addChild(arrow)
            timeline.fromTo(arrow.scale, { x: 0, y: 0 }, { duration: 0.2, x: 0.25, y: 0.25, ease: 'quad.out' }, 0)
            .fromTo(arrow, { x: unit.x, y: unit.y, rotation: Math.atan2(target.y - unit.y, target.x - unit.x) }, { duration: 0.3, x: target.x, y: target.y, ease: 'none' }, 0)
            .fromTo(arrow, { alpha: 1 }, { duration: 0.2, alpha: 0, ease: 'quad.in' }, 0.3)
            .add(() => arrow.destroy(), 0.5)
    
            timeline.addLabel('hit', 0.3)
    
            if(options.trail){
                const effect = new PIXI.Sprite(assets[options.trail])
                effect.zIndex = 12
                effect.anchor.set(0.5, 0.5)
                effect.blendMode = PIXI.BLEND_MODES.ADD
                unit.parent.addChild(effect)
                timeline.fromTo(effect.scale, { x: 0, y: 0 }, { duration: 0.2, x: 0.8, y: 0.8, ease: 'quad.out' }, 0)
                .fromTo(effect, { x: unit.x, y: unit.y, rotation: Math.atan2(target.y - unit.y, target.x - unit.x) }, { duration: 0.3, x: target.x, y: target.y, ease: 'none' }, 0)
                .fromTo(effect, { alpha: 1 }, { duration: 0.2, alpha: 0, ease: 'quad.in' }, 0.3)
                .add(() => effect.destroy(), 0.5)
            }
    
            if(options.impact){
                const explosion = new PIXI.Sprite(assets[options.impact])
                explosion.zIndex = 12
                explosion.anchor.set(0.5)
                explosion.blendMode = PIXI.BLEND_MODES.ADD
                unit.parent.addChild(explosion)
                timeline.fromTo(explosion, { x: target.x, y: target.y, alpha: 1 }, { duration: 0.4, alpha: 0, ease: 'quad.in' }, 0.3)
                .fromTo(explosion.scale, { x: 0, y: 0 }, { duration: 0.4, x: 1.2, y: 1.2, ease: 'cubic.out' }, 0.3)
                .add(() => explosion.destroy(), 0.7)
            }
        })

        return timeline
    }
}