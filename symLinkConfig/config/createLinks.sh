#!/bin/bash

rm -rf appsettings.json subdir*
ln -r -s subdir1/appsettings.json appsettings.json
mkdir subdir1
ln -r -s subdir2/appsettings.json subdir1/appsettings.json
ln -r -s subdir3 subdir2
mkdir subdir3
ln -r -s subdir4/symlink_target_file.json subdir3/appsettings.json
ln -r -s ../symlink_target_dir subdir4
