const fs = require('fs')
const path = require('path')

const directory = path.resolve(__dirname, '.')

let output = 'var assets={'
fs.readdirSync(directory, { withFileTypes: true }).forEach(file => {
    const extension = path.extname(file.name)
    if(!/\.(jpg|png)/i.test(extension)) return
    console.log(`packing: ${file.name}`)
    const base64 = `data:image/${extension.slice(1)};base64,${fs.readFileSync(path.join(directory, file.name), { encoding: 'base64' })}`
    output += `'${path.basename(file.name, extension)}':PIXI.Texture.from(Object.assign(new Image(),{src:'${base64}'})),`
})
output += '}'

fs.writeFileSync(path.join(directory, 'assets.js'), output, 'utf8')
console.log('done')