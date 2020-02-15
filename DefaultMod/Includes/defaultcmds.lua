concommand.Add("help", function(cmd, args)
	print("Currently available commands:")
	for k,v in pairs(concommand.GetTable()) do
		print(k)
	end
end)

concommand.Add("lua_run_cl", function(cmd, args, argstr)
	loadstring(argstr)()
end)

TEXT_ALIGN_BOTTOM = 0
TEXT_ALIGN_CENTER = 1
TEXT_ALIGN_RIGHT = 2
TEXT_ALIGN_LEFT = 3
TEXT_ALIGN_TOP = 4