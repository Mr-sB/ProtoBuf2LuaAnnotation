---@class Prefix_example_TestEnum
local Prefix_example_TestEnum = {
    Enum0 = 0,
    Enum1 = 1,
    Enum2 = 2,
    Enum3 = 3,
    Enum100 = 100,
}

---@class Prefix_example_TestMessage1
---@field public str string
---@field public number1 number
---@field public number2 number
---@field public boolean boolean
---@field public message1 Prefix_example_TestMessage2
---@field public booleanArray boolean[]
---@field public map1 table<string, number>
---@field public map2 table<number, Prefix_example_TestMessage2>
local Prefix_example_TestMessage1 = nil

---@class Prefix_example_TestMessage2
---@field public str string
---@field public number1 number
---@field public number2 number
---@field public boolean boolean
local Prefix_example_TestMessage2 = nil

return {
    Prefix_example_TestEnum = Prefix_example_TestEnum,
}
