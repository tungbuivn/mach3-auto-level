## software requires
- Flatcam beta

## hardware require
- mach3 controller

![Screenshoot](https://github.com/tungbuivn/mach3-auto-level/blob/main/elec/my-app/images/main.png?raw=true)


## NOTE:
currently not work with arc, only line segments can be process

## workflow

## process gerber file
```shell
cd directory_with_gerber_file
GCodeProcess.exe ger
```
file will be auto generate:


| File         | Desc                                        | 
|:-------------|:--------------------------------------------|
| cutout_nc.nc | cut board nc file                           | 
| drill06.nc   | drill with knife 0.6mm                      |
| drill08.nc   | drill with knife 0.8mm                      |
| ncc_board.nc | bottom layer                                |
| rpf.nc       | nc script using to collect height map point |



after collect map point call this command to update z-height of all nc file
```shell
GCodeProcess.exe map point_map_file
```
this command will update above nc file to new file with the height modified

| File               | desc                                        | 
|:-------------------|:--------------------------------------------|
| level_cutout_nc.nc ||
| level_drill06.nc   ||
| level_drill08.nc   ||
| level_ncc_board.nc ||

To process top layer
```shell
GCodeProcess.exe ger -t 1
```
## fusion 360 gcode process
this will repair fusion 360 gcode file
```shell
GCodeProcess.exe 360 gcode_file
```
