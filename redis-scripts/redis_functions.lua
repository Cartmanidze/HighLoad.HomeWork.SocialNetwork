#!lua name=dialogLib

local function escape_json_string(s)
    s = string.gsub(s, "\\", "\\\\")
    s = string.gsub(s, "\"", "\\\"")
    s = string.gsub(s, "\n", "\\n")
    return s
end

local function encode_table_to_json(t)
    local parts = {}
    table.insert(parts, "{")
    local first = true
    for k, v in pairs(t) do
        if not first then
            table.insert(parts, ",")
        else
            first = false
        end
        table.insert(parts, "\"")
        table.insert(parts, escape_json_string(k))
        table.insert(parts, "\":\"")
        table.insert(parts, escape_json_string(v))
        table.insert(parts, "\"")
    end
    table.insert(parts, "}")
    return table.concat(parts)
end

local function encode_list_of_tables(list)
    local parts = {}
    table.insert(parts, "[")
    local first = true
    for _, msgTable in ipairs(list) do
        if not first then
            table.insert(parts, ",")
        else
            first = false
        end
        table.insert(parts, encode_table_to_json(msgTable))
    end
    table.insert(parts, "]")
    return table.concat(parts)
end

local function get_dialog_key(user1, user2)
    if user1 < user2 then
        return "dialog:" .. user1 .. ":" .. user2
    else
        return "dialog:" .. user2 .. ":" .. user1
    end
end

redis.register_function('send_message', function(keys, args)
    local msgId      = args[1]
    local senderId   = args[2]
    local receiverId = args[3]
    local text       = args[4]

    local now = redis.call("TIME")
    local createdAtSec = tonumber(now[1])

    local messageKey = "message:" .. msgId
    local dialogKey  = get_dialog_key(senderId, receiverId)

    redis.call("HSET", messageKey,
        "id", msgId,
        "senderId", senderId,
        "receiverId", receiverId,
        "text", text,
        "createdAt", createdAtSec
    )

    redis.call("ZADD", dialogKey, createdAtSec, msgId)

    return msgId
end)

redis.register_function('get_dialog', function(keys, args)
    local userId  = args[1]
    local otherId = args[2]

    local dialogKey = get_dialog_key(userId, otherId)

    local msgIds = redis.call("ZRANGE", dialogKey, 0, -1)
    if #msgIds == 0 then
        return "[]"
    end

    local result = {}
    for _, msgId in ipairs(msgIds) do
        local h = redis.call("HGETALL", "message:" .. msgId)
        if #h > 0 then
            local msgTable = {}
            for i = 1, #h, 2 do
                local field = h[i]
                local val   = h[i + 1]
                msgTable[field] = val
            end
            table.insert(result, msgTable)
        end
    end

    return encode_list_of_tables(result)
end)
