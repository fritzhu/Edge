StairLights.State.OnChange(function(old, new)
	if LoungeLights.State.Get() ~= new then
		trace('Tying lounge lights to stair lights =', new)
		LoungeLights.Switch(new)
	end
end)

LoungeLights.State.OnChange(function(old, new)
	if StairLights.State.Get() ~= new then
		trace('Tying stair lights to lounge lights =', new)
		StairLights.Switch(new)
	end
end)


LoungeLights.State.OnChange(function(old, new) 
	trace('Clock.IsAfterSunset =', Clock.IsAfterSunset.Get())
	trace('LoungeLights.State =', new)
	if (new == true) and (Clock.IsAfterSunset.Get() == true) then
		info('Lounge lights switched on after sunset, opening scene Evening')
		All.Evening()
	end
end)

StairLights.State.OnChange(function(old, new) 
	trace('Clock.IsAfterBedtime =', Clock.IsAfterBedtime.Get())
	trace('StairLights.State =', new)
	if (new == false) and (Clock.IsAfterBedtime.Get() == true) then
		info('Stair lights switched off after bedtime, opening scene Goodnight')
		All.Goodnight()
	end
end)