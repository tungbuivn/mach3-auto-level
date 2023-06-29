## software requires
- Flatcam beta

## hardware require
- mach3 controller

![Screenshoot](https://github.com/tungbuivn/mach3-auto-level/blob/main/elec/my-app/images/main.png?raw=true)


## NOTE:

- currently not work with arc (G2,G3), only line segments can be process (G1)
- any drilling <=0.6 will be using 0.6mm milling tool
- any drilling in range 0.61 to 1.2 will be using 0.8mm milling tool, all milling hole <= 1.2mm will using 0.8mm to milling
- all other drill and mill will using 2mm milling tool 

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
GCodeProcess.exe map point_map_file some_nc_file
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
