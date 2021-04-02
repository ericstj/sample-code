del appsettings.json
rd /q /s subdir1 subdir2 subdir3 subdir4
mklink appsettings.json subdir1\appsettings.json
md subdir1
mklink subdir1\appsettings.json ..\subdir2\appsettings.json
mklink /D subdir2 subdir3
md subdir3
mklink subdir3\appsettings.json ..\subdir4\symlink_target_file.json
mklink /D subdir4 ..\symlink_target_dir