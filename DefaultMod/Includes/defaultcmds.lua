concommand.Add("help", function(cmd, args)
	for k,v in pairs(args) do
		print(v)
	end
end)

concommand.Add("lua_run_cl", function(cmd, args)
	print(args)
end)