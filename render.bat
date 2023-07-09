set output="C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\pearlcat\world\t1-rooms"

set directory_to_render="%UserProfile%\Desktop\Level Editor\LevelEditorProjects"

set drizzle_exe="%UserProfile%\Desktop\Drizzle\Drizzle.ConsoleApp.exe"
set drizzle_output="%UserProfile%\Desktop\Drizzle\Data\Levels"

for /F "delims=" %%i in ('dir %directory_to_render%\*.txt /b') do call set "to_render=%%to_render%% %directory_to_render%\%%i"

%drizzle_exe% render %to_render%
xcopy /s /Y %drizzle_output% %output%
PAUSE