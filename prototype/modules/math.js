const vec2 = (x = 0, y = x) => vec2.set(x, y, new Float32Array(2))
vec2.set = (x, y, out) => {
    out[0] = x
    out[1] = y
    return out
}
vec2.add = (a, b, out) => {
    out[0] = a[0] + b[0]
    out[1] = a[1] + b[1]
    return out
}

vec2.cardinal = [
    vec2(1,0),
    vec2(0,-1),
    vec2(-1,0),
    vec2(0,1)
]

Array.range = function(count){
    const out = Array(count)
    for(let i = 0; i < count; i++) out[i] = i
    return out
}
Array.prototype.shuffle = function(random = Math.random){
    for(let i = this.length; i; i--){
        let j = Math.floor(random() * i)
        let temp = this[j]
        this[j] = this[i - 1]
        this[i - 1] = temp
    }
    return this
}



class XorShift {
    constructor(seed){
        this._state0U = seed[0] | 0
        this._state0L = seed[1] | 0
        this._state1U = seed[2] | 0
        this._state1L = seed[3] | 0
    }
    randomInt64(){
        // uint64_t s1 = s[0]
        var s1U = this._state0U, s1L = this._state0L;
        // uint64_t s0 = s[1]
        var s0U = this._state1U, s0L = this._state1L;

        // result = s0 + s1
        var sumL = (s0L >>> 0) + (s1L >>> 0);
        var resU = (s0U + s1U + (sumL / 2 >>> 31)) >>> 0;
        var resL = sumL >>> 0;

        // s[0] = s0
        this._state0U = s0U;
        this._state0L = s0L;

        // - t1 = [0, 0]
        var t1U = 0, t1L = 0;
        // - t2 = [0, 0]
        var t2U = 0, t2L = 0;

        // s1 ^= s1 << 23;
        // :: t1 = s1 << 23
        var a1 = 23;
        var m1 = 0xFFFFFFFF << (32 - a1);
        t1U = (s1U << a1) | ((s1L & m1) >>> (32 - a1));
        t1L = s1L << a1;
        // :: s1 = s1 ^ t1
        s1U = s1U ^ t1U;
        s1L = s1L ^ t1L;

        // t1 = ( s1 ^ s0 ^ ( s1 >> 17 ) ^ ( s0 >> 26 ) )
        // :: t1 = s1 ^ s0
        t1U = s1U ^ s0U;
        t1L = s1L ^ s0L;
        // :: t2 = s1 >> 18
        var a2 = 18;
        var m2 = 0xFFFFFFFF >>> (32 - a2);
        t2U = s1U >>> a2;
        t2L = (s1L >>> a2) | ((s1U & m2) << (32 - a2));
        // :: t1 = t1 ^ t2
        t1U = t1U ^ t2U;
        t1L = t1L ^ t2L;
        // :: t2 = s0 >> 5
        var a3 = 5;
        var m3 = 0xFFFFFFFF >>> (32 - a3);
        t2U = s0U >>> a3;
        t2L = (s0L >>> a3) | ((s0U & m3) << (32 - a3));
        // :: t1 = t1 ^ t2
        t1U = t1U ^ t2U;
        t1L = t1L ^ t2L;

        // s[1] = t1
        this._state1U = t1U;
        this._state1L = t1L;

        // return result
        return [resU, resL];
    }
    random = () => {
        var t2 = this.randomInt64()
        // Math.pow(2, -32) = 2.3283064365386963e-10
        // Math.pow(2, -52) = 2.220446049250313e-16
        return t2[0] * 2.3283064365386963e-10 + (t2[1] >>> 12) * 2.220446049250313e-16;
    }
}
Math.random = new XorShift([0x24F, 0x837436, 0xAA34, 0x02]).random