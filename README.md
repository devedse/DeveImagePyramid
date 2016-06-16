# DeveImagePyramid

This tool can be used to generate an Image Pyramid. This can then be viewed using for example:
http://openseadragon.github.io/

### Image Pyramids

Image Pyramid's are basically huge images that are splitted into smaller files. E.g. 15\0_0.png contains the top most tile of the image.
When zooming out on the image it will gradually load smaller and smaller images (e.g. 8\0_0.png contains almost the whole image).

### Restrictions

Some restrictions for this tool when it is completed (note: it currently doesn't do anything yet :) ):

Input folder restrictions:
Name should be .....\15\images (so 0_0.png and 0_1.png etc should be in this folder). The program will generate the pyramid above by scaling the images.
TileSize or ImageSize is always 256 pixels wide and 256 pixels high.
BytesPerPixel should be 3 in the format R G B
Bits per sample should be 8

### Description of this tool and why it exists

The advantage of this tool over using for example VIPS is that this tool supports infinitely big images. VIPS will still have to load large sections of the Image into memory which (in my case) led to int overflows.
The reason for this is the tool actually using the already splitted images and only having a few of them in memory at a time.