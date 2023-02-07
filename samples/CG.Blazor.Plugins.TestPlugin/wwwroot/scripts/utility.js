// Generate a random number within a specified range
const getRandomNumberInRange = (lower = 0, upper = 10) => {
    if (isNaN(lower) || isNaN(upper)) {
        console.error("lower and upper must be valid numbers")
        return
    }
    lower = Math.ceil(lower)
    upper = Math.floor(upper)
    return Math.floor(Math.random() * (upper - lower + 1)) + lower
}

// Do something x times. e.g. times(3, () => console.log("hello"))
const times = (times, func) => {
    if (isNaN(times)) {
        console.error("times to run must be specified")
        return
    }
    if (typeof func !== "function") {
        console.error(`func must be a valid function, ${typeof func} provided`)
        return
    }
    Array.from(Array(times)).forEach(() => {
        func()
    })
}

// Shorten a string with ellipsis. e.g. shorten("I am some text", 4, 2) => I am..
const shorten = (text, length = 10, ellipsisCount = 3) => {
    if (!(typeof text === "string" || text instanceof String)) {
        console.error(`expecting a string, ${typeof text} provided`)
        return ""
    }
    if (isNaN(length) || isNaN(ellipsisCount)) {
        console.error("length and ellipsisCount must be valid numbers")
        return
    }

    if (text.length <= length) {
        return text
    }
    const ellipsis = Array.from(Array(ellipsisCount)).map(() => ".").join("")
    return `${text.substr(0, length)}${ellipsis}`
}

// Remove duplicates from an array. e.g. removeDuplicates(["Tom", "Simon", "Tom", "Sarah"]) => ["Tom", "Simon", "Sarah"]
const removeDuplicates = (arr) => {
    if (!Array.isArray(arr)) {
        console.error(`array expected, ${typeof arr} provided`)
        return
    }
    return [...new Set(arr)]
}

// Debounce (or delay) a function
const debounce = (func, timeout = 2500) => {
    if (typeof func !== "function") {
        console.error(`func must be a valid function, ${typeof func} provided`)
        return
    }
    let timer
    return (...args) => {
        clearTimeout(timer)
        timer = setTimeout(() => {
            func.apply(this, args)
        }, timeout)
    }
}

// Measure the performance/time taken of a function. e.g. measureTime("findPeople", someExpensiveFindPeopleFunction) => findPeople: 13426.336181640625ms
const measureTime = (func, label = "default") => {
    if (typeof func !== "function") {
        console.error(`func must be a valid function, ${typeof func} provided`)
        return
    }
    console.time(label)
    func()
    console.timeEnd(label)
}

// Slugify a string. e.g. slugify("Hello, everyone!") => hello-everyone
const slugify = (text) => {
    if (!(typeof text === "string" || text instanceof String)) {
        console.error(`string expected, ${typeof str} provided`)
        return str
    }
    return text.toLowerCase()
        .replace(/ /g, "-")
        .replace(/[^\w-]+/g, "")
}

// Camel case to snake case. e.g. camelToSnakeCase("camelCaseToSnakeCase") => camel_case_to_snake_case
const camelToSnakeCase = (text) => {
    if (!(typeof text === "string" || text instanceof String)) {
        console.error(`string expected, ${typeof text} provided`)
        return text
    }
    return text.replace(/[A-Z]/g, (letter) => `_${letter.toLowerCase()}`)
}

// Snake case to camel case. e.g. snakeToCamelCase("snake_case_to_camel_case") => snakeCaseToCamelCase
const snakeToCamelCase = (text) => {
    if (!(typeof text === "string" || text instanceof String)) {
        console.error(`string expected, ${typeof text} provided`)
        return text
    }
    text
        .toLowerCase()
        .replace(/([-_][a-z])/g, group =>
            group
                .toUpperCase()
                .replace("-", "")
                .replace("_", "")
        )
}

// Validate an email address. e.g. emailIsValid("somebody@somewhere.com") => true | emailIsValid("nobody@nowhere") => false
const emailIsValid = (email) => {
    if (!(typeof email === "string" || email instanceof String)) {
        console.error(`string expected, ${typeof email} provided`)
        return false
    }
    const expression = /\S+@\S+\.\S+/
    return expression.test(email)
}