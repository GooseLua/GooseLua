local string = string
local Msg = Msg

--[[---------------------------------------------------------
   Name: concommand
   Desc: A module to take care of the registration and calling
         of Lua console commands.
-----------------------------------------------------------]]
concommand = {}

local CommandList = {}
local CompleteList = {}

--[[---------------------------------------------------------
   Name: concommand.GetTable( )
   Desc: Returns the table of console commands and auto complete
-----------------------------------------------------------]]
function concommand.GetTable()
	return CommandList, CompleteList
end

--[[---------------------------------------------------------
   Name: concommand.Add( name, func, completefunc )
   Desc: Register a new console command
-----------------------------------------------------------]]
function concommand.Add( name, func, completefunc, help )
	local LowerName = string.lower( name )
	CommandList[ LowerName ] = func
	CompleteList[ LowerName ] = completefunc
	AddConsoleCommand( name, help )
end

--[[---------------------------------------------------------
   Name: concommand.Remove( name )
   Desc: Removes a console command
-----------------------------------------------------------]]
function concommand.Remove( name )
	local LowerName = string.lower( name )
	CommandList[ LowerName ] = nil
	CompleteList[ LowerName ] = nil
end

--[[---------------------------------------------------------
   Name: concommand.Run( )
   Desc: Called by the engine when an unknown console command is run
-----------------------------------------------------------]]
function concommand.Run( command, arguments, args )

	local LowerCommand = string.lower( command )

	if ( CommandList[ LowerCommand ] ~= nil ) then
		CommandList[ LowerCommand ]( command, arguments, args )
		return true
	end

	Msg( "Unknown command: " .. command .. "\n" )

	return false
end

--[[---------------------------------------------------------
   Name: concommand.AutoComplete( )
   Desc: Returns a table for the autocompletion
-----------------------------------------------------------]]
function concommand.AutoComplete( command, arguments )

	local LowerCommand = string.lower( command )

	if ( CompleteList[ LowerCommand ] ~= nil ) then
		return CompleteList[ LowerCommand ]( command, arguments )
	end

end
