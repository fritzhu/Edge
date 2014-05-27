StairLights.State.OnChange(function(old, new)
	if LoungeLights.State.Get() ~= new then
		info('StairLights toggled, mirroring State to LoungeLights')
		LoungeLights.Switch(new)
	end
end)

LoungeLights.State.OnChange(function(old, new)
	if StairLights.State.Get() ~= new then
		info('LoungeLights toggled, mirroring State to StairLights')
		StairLights.Switch(new)
	end
end)


LoungeLights.State.OnChange(function(old, new) 
	if (new == true) and (Clock.IsAfterSunset.Get() == true) then
		info('LoungeLights switched on after sunset, opening scene Evening')
		All.Evening()
	end
end)

StairLights.State.OnChange(function(old, new) 
	if (new == false) and (Clock.IsAfterBedtime.Get() == true) then
		info('StairLights switched off after bedtime, opening scene Goodnight')
		All.Goodnight()
	end
end)

StairLights.State.OnChange(function(old, new) 
	if (new == true) and (Clock.IsBeforeSunrise.Get() == true) then
		info('StairLights switched on before sunrise, opening scene GoodMorning')
		All.GoodMorning()
	end
end)