# System.Drawing.Extensions - Algorithm Reference

## Color Space Conversion

### Standard Color Models

- [X] [RGB](https://en.wikipedia.org/wiki/RGB_color_model) - Additive color model (byte and normalized variants) | [impl](http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html), [code](https://github.com/colour-science/colour)
- [X] [CIE XYZ](https://en.wikipedia.org/wiki/CIE_1931_color_space) - CIE 1931 standard | [impl](http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html), [code](https://github.com/colour-science/colour)
- [X] [CMYK](https://en.wikipedia.org/wiki/CMYK_color_model) - Subtractive color model | [impl](https://www.rapidtables.com/convert/color/rgb-to-cmyk.html), [code](https://github.com/color-js/color.js)

### Perceptual Color Spaces

- [X] [CIE L\*a\*b\*](https://en.wikipedia.org/wiki/CIELAB_color_space) - CIE 1976 perceptually uniform | [impl](http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_Lab.html), [code](https://github.com/colour-science/colour)
- [X] [CIE L\*u\*v\*](https://en.wikipedia.org/wiki/CIELUV) - CIE 1976 perceptually uniform (for emissive colors) | [impl](http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_Luv.html), [code](https://github.com/colour-science/colour)
- [X] [CIE LCh](https://en.wikipedia.org/wiki/CIELAB_color_space#Cylindrical_representation:_CIELCh_or_CIEHLC) - Cylindrical Lab (polar coordinates) | [impl](http://www.brucelindbloom.com/index.html?Eqn_Lab_to_LCH.html)
- [X] [DIN99](https://de.wikipedia.org/wiki/DIN99-Farbraum) - DIN 6176:2001 German industrial standard | [impl](https://github.com/colour-science/colour/blob/develop/colour/models/din99.py)
- [X] [Hunter Lab](https://en.wikipedia.org/wiki/CIELAB_color_space#Hunter_Lab) - Richard S. Hunter 1948 | [impl](http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_Hunter_Lab.html)
- [X] [Oklab](https://bottosson.github.io/posts/oklab/) - Björn Ottosson 2020 ([interactive](https://bottosson.github.io/misc/colorpicker/)) | [impl](https://bottosson.github.io/posts/oklab/), [code](https://github.com/bottosson/bottosson.github.io/blob/master/misc/colorpicker/colorconversion.js)

### HDR/WCG Color Spaces

- [X] [JzAzBz](https://observablehq.com/@jrus/jzazbz) - Safdar et al. 2017 ([paper](https://www.osapublishing.org/oe/fulltext.cfm?uri=oe-25-13-15131)) | [impl](https://observablehq.com/@jrus/jzazbz), [code](https://github.com/colour-science/colour/blob/develop/colour/models/jzazbz.py)
- [X] [JzCzhz](https://observablehq.com/@jrus/jzazbz) - Cylindrical JzAzBz (polar coordinates) | [impl](https://observablehq.com/@jrus/jzazbz)
- [X] [ICtCp](https://en.wikipedia.org/wiki/ICtCp) - Dolby 2016 (ITU-R BT.2100) | [impl](https://professional.dolby.com/siteassets/pdfs/ictcp_dolbywhitepaper_v071.pdf), [code](https://github.com/colour-science/colour/blob/develop/colour/models/rgb/ictcp.py)
- [X] [Rec. 2100](https://en.wikipedia.org/wiki/Rec._2100) - ITU-R BT.2100 HDR/WCG broadcast (PQ/HLG transfer) | [spec](https://www.itu.int/rec/R-REC-BT.2100), [impl](https://colour.readthedocs.io/en/latest/generated/colour.models.eotf_ST2084.html)
- [X] [sRGB Linear](https://en.wikipedia.org/wiki/SRGB) - IEC 61966-2-1 (linear working space) | [impl](http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html), [code](https://www.w3.org/Graphics/Color/srgb)

### Wide Gamut Color Spaces

- [X] [Display P3](https://en.wikipedia.org/wiki/DCI-P3) - Apple 2015 / DCI-P3 D65 variant | [impl](http://www.brucelindbloom.com/index.html?WorkingSpaceInfo.html), [code](https://github.com/color-js/color.js/blob/main/src/spaces/p3.js)
- [X] [ProPhoto RGB](https://en.wikipedia.org/wiki/ProPhoto_RGB_color_space) - Kodak 1999 (ROMM RGB, ultra-wide) | [impl](http://www.brucelindbloom.com/index.html?WorkingSpaceInfo.html), [spec](https://www.color.org/ROMMRGB.pdf)
- [X] [ACEScg](https://en.wikipedia.org/wiki/Academy_Color_Encoding_System) - Academy 2014 (VFX/animation linear) | [spec](https://github.com/ampas/aces-dev), [impl](https://docs.acescentral.com/)
- [X] [Adobe RGB (1998)](https://en.wikipedia.org/wiki/Adobe_RGB_color_space) - Adobe 1998 (wide gamut for print) | [impl](http://www.brucelindbloom.com/index.html?WorkingSpaceInfo.html), [spec](https://www.adobe.com/digitalimag/adobergb.html)

### Cylindrical Color Spaces

- [X] [HSV/HSB](https://en.wikipedia.org/wiki/HSL_and_HSV) - Smith 1978 (Hue, Saturation, Value) | [impl](https://www.rapidtables.com/convert/color/rgb-to-hsv.html), [code](https://github.com/colorjs/color.js/blob/main/src/spaces/hsv.js)
- [X] [HSL](https://en.wikipedia.org/wiki/HSL_and_HSV) - Joblove & Greenberg 1978 (Hue, Saturation, Lightness) | [impl](https://www.rapidtables.com/convert/color/rgb-to-hsl.html), [code](https://github.com/colorjs/color.js/blob/main/src/spaces/hsl.js)
- [X] [HWB](https://en.wikipedia.org/wiki/HWB_color_model) - Smith & Lyons 1996 (Hue, Whiteness, Blackness) | [impl](https://www.w3.org/TR/css-color-4/#the-hwb-notation), [code](https://github.com/colorjs/color.js/blob/main/src/spaces/hwb.js)

### Luminance-Chrominance Color Spaces

- [X] [YCbCr](https://en.wikipedia.org/wiki/YCbCr) - ITU-R BT.601/709 digital video | [impl](https://www.itu.int/rec/R-REC-BT.601), [code](https://github.com/colour-science/colour/blob/develop/colour/models/rgb/ycbcr.py)
- [X] [YUV](https://en.wikipedia.org/wiki/YUV) - PAL/SECAM analog video | [impl](https://www.fourcc.org/fccyvrgb.php), [code](https://softpixel.com/~cwright/programming/colorspace/yuv/)

### Color Appearance Models

Reference: [Fairchild 2013](https://www.wiley.com/en-us/Color+Appearance+Models,+3rd+Edition-p-9781119967033) - Color Appearance Models (3rd ed.)

- [ ] [CIECAM02](https://en.wikipedia.org/wiki/CIECAM02) - CIE 2002 standard CAM (Li et al.) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.CIECAM02.html), [wiki](https://en.wikipedia.org/wiki/CIECAM02)
- [ ] [CIECAM16](https://observablehq.com/@jrus/cam16) - Li et al. 2017 (improved successor to CIECAM02) | [paper](https://doi.org/10.1002/col.22131), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.CIECAM16.html)
- [ ] [CAM16](https://observablehq.com/@jrus/cam16) - Li et al. 2017 (correlates from CIECAM16) | [impl](https://observablehq.com/@jrus/cam16), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.CAM16.html)
- [ ] [ZCAM](https://www.osapublishing.org/oe/fulltext.cfm?uri=oe-29-4-6036) - Safdar et al. 2021 (HDR CAM based on JzAzBz) | [paper](https://doi.org/10.1364/OE.413659), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.ZCAM.html)
- [ ] [Kim2009](https://www.sciencedirect.com/science/article/abs/pii/S0042698909002260) - Kim et al. 2009 (HDR-capable CAM) | [paper](https://doi.org/10.1016/j.visres.2009.06.009), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.Kim2009.html)
- [ ] [Hellwig2022](https://www.osapublishing.org/josaa/abstract.cfm?uri=josaa-39-7-1232) - Hellwig & Fairchild 2022 (improved chroma) | [paper](https://doi.org/10.1364/JOSAA.450753), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.Hellwig2022.html)
- [ ] [ATD95](https://doi.org/10.1002/col.5080200504) - Guth 1995 (visual threshold model) | [paper](https://doi.org/10.1002/col.5080200504), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.ATD95.html)
- [ ] [LLAB](https://doi.org/10.1002/col.5080210105) - Luo & Rigg 1996 (Leeds Lab model) | [paper](https://doi.org/10.1002/col.5080210105), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.LLAB.html)
- [ ] [Nayatani95](https://doi.org/10.1002/col.5080200509) - Nayatani et al. 1995 (chromatic adaptation) | [paper](https://doi.org/10.1002/col.5080200509), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.Nayatani95.html)
- [ ] [RLAB](https://doi.org/10.1002/col.5080200607) - Fairchild 1996 (reference viewing conditions) | [paper](https://doi.org/10.1002/col.5080200607), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.RLAB.html)
- [ ] [Hunt](https://doi.org/10.1002/col.5080190106) - Hunt 1987/1994 (complex physiological model) | [paper](https://doi.org/10.1002/col.5080190106), [colour](https://colour.readthedocs.io/en/latest/generated/colour.appearance.Hunt.html)

### CAM-Based Uniform Color Spaces

Reference: [Luo et al. 2006](https://doi.org/10.1002/col.20227) - Uniform Colour Spaces Based on CIECAM02

- [ ] [CAM02-UCS](https://doi.org/10.1002/col.20227) - Luo et al. 2006 (uniform from CIECAM02) | [paper](https://doi.org/10.1002/col.20227), [colour](https://colour.readthedocs.io/en/latest/generated/colour.CAM02UCS_to_XYZ.html)
- [ ] [CAM02-LCD](https://doi.org/10.1002/col.20227) - Large Color Difference variant | [paper](https://doi.org/10.1002/col.20227), [colour](https://colour.readthedocs.io/en/latest/generated/colour.CAM02LCD_to_XYZ.html)
- [ ] [CAM02-SCD](https://doi.org/10.1002/col.20227) - Small Color Difference variant | [paper](https://doi.org/10.1002/col.20227), [colour](https://colour.readthedocs.io/en/latest/generated/colour.CAM02SCD_to_XYZ.html)
- [ ] [CAM16-UCS](https://en.wikipedia.org/wiki/Color_appearance_model#CIECAM16) - Li et al. 2017 (uniform from CIECAM16) | [impl](https://observablehq.com/@jrus/cam16), [colour](https://colour.readthedocs.io/en/latest/generated/colour.CAM16UCS_to_XYZ.html)
- [ ] [CAM16-LCD](https://doi.org/10.1002/col.22131) - Large Color Difference variant | [paper](https://doi.org/10.1002/col.22131), [colour](https://colour.readthedocs.io/en/latest/generated/colour.CAM16LCD_to_XYZ.html)
- [ ] [CAM16-SCD](https://doi.org/10.1002/col.22131) - Small Color Difference variant | [paper](https://doi.org/10.1002/col.22131), [colour](https://colour.readthedocs.io/en/latest/generated/colour.CAM16SCD_to_XYZ.html)

### CIE Uniform Chromaticity Scales

Reference: [CIE 015:2018](https://cie.co.at/publications/colorimetry-4th-edition) - Colorimetry (4th ed.)

- [ ] [CIE 1960 UCS](https://en.wikipedia.org/wiki/CIE_1960_color_space) - MacAdam 1937 (u,v uniform chromaticity) | [wiki](https://en.wikipedia.org/wiki/CIE_1960_color_space), [colour](https://colour.readthedocs.io/en/latest/generated/colour.UCS_to_XYZ.html)
- [ ] [CIE 1964 U\*V\*W\*](https://en.wikipedia.org/wiki/CIE_1964_color_space) - CIE 1964 (predecessor to CIELAB) | [wiki](https://en.wikipedia.org/wiki/CIE_1964_color_space), [colour](https://colour.readthedocs.io/en/latest/generated/colour.UVW_to_XYZ.html)
- [ ] [CIE 1976 UCS](https://en.wikipedia.org/wiki/CIELUV#The_forward_transformation) - CIE 1976 (u',v' uniform chromaticity) | [wiki](https://en.wikipedia.org/wiki/CIELUV#The_forward_transformation), [colour](https://colour.readthedocs.io/en/latest/generated/colour.Luv_uv_to_xy.html)

### Additional Perceptual Color Spaces

- [ ] [ICaCb](https://www.osapublishing.org/josaa/abstract.cfm?uri=josaa-24-6-1823) - Colorfulness-adaptive coordinates | [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_ICaCb.html)
- [ ] [IgPgTg](https://www.osapublishing.org/josaa/fulltext.cfm?uri=josaa-29-1-117) - Perceptual quantizer-based (like ICtCp) | [paper](https://doi.org/10.1364/JOSAA.29.000117), [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_IgPgTg.html)
- [ ] [OSA-UCS](https://en.wikipedia.org/wiki/OSA-UCS) - Optical Society 1974 (cubo-octahedral lattice) | [wiki](https://en.wikipedia.org/wiki/OSA-UCS), [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_OSA_UCS.html)
- [ ] [ProLab](https://www.osapublishing.org/josaa/abstract.cfm?uri=josaa-38-8-1136) - ProPhoto-based Lab | [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_ProLab.html)
- [ ] [IPT Ragoo2021](https://doi.org/10.1002/col.22665) - Ragoo & Westland 2021 (improved IPT) | [paper](https://doi.org/10.1002/col.22665), [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_IPT_Ragoo2021.html)
- [ ] [Yrg](https://www.color.org/specification/ICC.2-2019.pdf) - iCAM06/ICC.2 opponent-color | [spec](https://www.color.org/specification/ICC.2-2019.pdf), [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_Yrg.html)
- [ ] [hdr-CIELAB](https://www.researchgate.net/publication/250272225_Encoding_High_Dynamic_Range_Scene-Referred_Image_Data) - Fairchild & Chen 2011 (HDR Lab) | [paper](https://doi.org/10.1002/col.20639), [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_hdr_CIELab.html)
- [ ] [hdr-IPT](https://www.researchgate.net/publication/250272225_Encoding_High_Dynamic_Range_Scene-Referred_Image_Data) - Fairchild & Wyble 2010 (HDR IPT) | [paper](https://doi.org/10.1002/col.20639), [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_hdr_IPT.html)
- [ ] [Hunter Rdab](https://sensing.konicaminolta.asia/product-type/hunter-rdab/) - Hunter Rd,a,b scale (reflectance) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.XYZ_to_Hunter_Rdab.html)
- [ ] [IHLS](https://www.sciencedirect.com/science/article/abs/pii/S0167865598000450) - Improved HLS (Hanbury & Serra 2003) | [paper](https://doi.org/10.1016/S0167-8655(98)00045-0), [colour](https://colour.readthedocs.io/en/latest/generated/colour.RGB_to_IHLS.html)
- [ ] [Prismatic](https://doi.org/10.1016/j.isprsjprs.2017.09.006) - Leow & Cook 1999 (separates intensity) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.RGB_to_Prismatic.html)

### DIN99 Variants

Reference: [DIN 6176:2001](https://www.beuth.de/de/norm/din-6176/4238073) - German industrial color metric

- [ ] [DIN99b](https://doi.org/10.1002/col.20222) - Cui et al. 2002 (improved DIN99) | [paper](https://doi.org/10.1002/col.20222), [colour](https://colour.readthedocs.io/en/latest/generated/colour.Lab_to_DIN99.html)
- [ ] [DIN99c](https://doi.org/10.1002/col.20222) - DIN99 variant c (chromatic adaptation) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.Lab_to_DIN99.html)
- [ ] [DIN99d](https://doi.org/10.1002/col.20222) - DIN99 variant d (optimized for small ΔE) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.Lab_to_DIN99.html)

### Professional Cinema/Camera RGB Color Spaces

Reference: [colour-science RGB Colourspaces](https://colour.readthedocs.io/en/latest/generated/colour.RGB_COLOURSPACES.html)

#### ACES (Academy Color Encoding System)

- [ ] [ACES2065-1](https://en.wikipedia.org/wiki/Academy_Color_Encoding_System) - Academy 2014 (scene-referred, base encoding) | [spec](https://github.com/ampas/aces-dev), [docs](https://docs.acescentral.com/)
- [ ] [ACEScc](https://docs.acescentral.com/specifications/acescc) - Academy (log encoding for color grading) | [spec](https://github.com/ampas/aces-dev), [colour](https://colour.readthedocs.io/)
- [ ] [ACEScct](https://docs.acescentral.com/specifications/acescct) - Academy (log + toe for grading) | [spec](https://github.com/ampas/aces-dev), [colour](https://colour.readthedocs.io/)
- [ ] [ACESproxy](https://docs.acescentral.com/specifications/acesproxy) - Academy (integer proxy for transmission) | [spec](https://github.com/ampas/aces-dev), [colour](https://colour.readthedocs.io/)

#### Camera Manufacturer Spaces

- [ ] [ARRI Wide Gamut 3](https://www.arri.com/en/learn-help/learn-help-camera-system/camera-workflow/image-science/color-science) - ARRI Alexa cameras | [ARRI](https://www.arri.com/), [colour](https://colour.readthedocs.io/)
- [ ] [ARRI Wide Gamut 4](https://www.arri.com/en/learn-help/learn-help-camera-system/camera-workflow/image-science/color-science) - ARRI ALEXA 35 (gen 5) | [ARRI](https://www.arri.com/), [colour](https://colour.readthedocs.io/)
- [ ] [RED Wide Gamut RGB](https://www.red.com/red-101/color-management-white-paper) - RED cameras (REDcolor4) | [RED](https://www.red.com/), [colour](https://colour.readthedocs.io/)
- [ ] [S-Gamut](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony cameras (original) | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [S-Gamut3](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony cameras (improved) | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [S-Gamut3.Cine](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony (cinema-optimized primaries) | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [Venice S-Gamut3](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony Venice cameras | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [Cinema Gamut](https://www.usa.canon.com/shop/p/canon-log-3-cine-lut-package) - Canon EOS Cinema cameras | [Canon](https://www.usa.canon.com/), [colour](https://colour.readthedocs.io/)
- [ ] [V-Gamut](https://pro-av.panasonic.net/en/cinema_camera_varicam_702/index.html) - Panasonic VariCam | [Panasonic](https://pro-av.panasonic.net/), [colour](https://colour.readthedocs.io/)
- [ ] [N-Gamut](https://nikonimglib.com/nvnx/) - Nikon Z cameras | [Nikon](https://www.nikonusa.com/), [colour](https://colour.readthedocs.io/)
- [ ] [F-Gamut](https://fujifilm-x.com/en-us/stories/f-log-everything-you-need-to-know/) - Fujifilm X-T series | [Fujifilm](https://fujifilm-x.com/), [colour](https://colour.readthedocs.io/)
- [ ] [D-Gamut](https://enterprise-developer.dji.com/inspire-3/downloads) - DJI drone cameras | [DJI](https://www.dji.com/), [colour](https://colour.readthedocs.io/)

#### Post-Production Spaces

- [ ] [Blackmagic Wide Gamut](https://www.blackmagicdesign.com/products/davinciresolve) - Blackmagic DaVinci cameras | [BMD](https://www.blackmagicdesign.com/), [colour](https://colour.readthedocs.io/)
- [ ] [DaVinci Wide Gamut](https://www.blackmagicdesign.com/products/davinciresolve) - DaVinci Resolve grading | [BMD](https://www.blackmagicdesign.com/), [colour](https://colour.readthedocs.io/)
- [ ] [FilmLight E-Gamut](https://www.filmlight.ltd.uk/support/documents/colourspaces.php) - FilmLight Baselight | [FilmLight](https://www.filmlight.ltd.uk/), [colour](https://colour.readthedocs.io/)

### Transfer Functions (OETF/EOTF)

Reference: [ITU-R BT.2100](https://www.itu.int/rec/R-REC-BT.2100) - HDR TV systems

#### Log Encodings

- [ ] [ARRI LogC3](https://www.arri.com/en/learn-help/learn-help-camera-system/camera-workflow/image-science/color-science) - ARRI LogC (Extended Data) | [ARRI](https://www.arri.com/), [colour](https://colour.readthedocs.io/)
- [ ] [ARRI LogC4](https://www.arri.com/en/learn-help/learn-help-camera-system/camera-workflow/image-science/color-science) - ARRI ALEXA 35 log | [ARRI](https://www.arri.com/), [colour](https://colour.readthedocs.io/)
- [ ] [Canon Log](https://www.usa.canon.com/shop/p/canon-log-3-cine-lut-package) - Canon Log 1 (original) | [Canon](https://www.usa.canon.com/), [colour](https://colour.readthedocs.io/)
- [ ] [Canon Log 2](https://www.usa.canon.com/shop/p/canon-log-3-cine-lut-package) - Canon Log 2 (C300 MkII) | [Canon](https://www.usa.canon.com/), [colour](https://colour.readthedocs.io/)
- [ ] [Canon Log 3](https://www.usa.canon.com/shop/p/canon-log-3-cine-lut-package) - Canon Log 3 (current standard) | [Canon](https://www.usa.canon.com/), [colour](https://colour.readthedocs.io/)
- [ ] [S-Log](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony S-Log (original) | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [S-Log2](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony S-Log2 | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [S-Log3](https://pro.sony/ue_US/technology/s-log-s-gamut) - Sony S-Log3 (current standard) | [Sony](https://pro.sony/), [colour](https://colour.readthedocs.io/)
- [ ] [V-Log](https://pro-av.panasonic.net/en/cinema_camera_varicam_702/index.html) - Panasonic V-Log | [Panasonic](https://pro-av.panasonic.net/), [colour](https://colour.readthedocs.io/)
- [ ] [F-Log](https://fujifilm-x.com/en-us/stories/f-log-everything-you-need-to-know/) - Fujifilm F-Log | [Fujifilm](https://fujifilm-x.com/), [colour](https://colour.readthedocs.io/)
- [ ] [F-Log2](https://fujifilm-x.com/en-us/stories/f-log-everything-you-need-to-know/) - Fujifilm F-Log2 (X-H2S) | [Fujifilm](https://fujifilm-x.com/), [colour](https://colour.readthedocs.io/)
- [ ] [N-Log](https://nikonimglib.com/nvnx/) - Nikon N-Log | [Nikon](https://www.nikonusa.com/), [colour](https://colour.readthedocs.io/)
- [ ] [D-Log](https://enterprise-developer.dji.com/inspire-3/downloads) - DJI D-Log | [DJI](https://www.dji.com/), [colour](https://colour.readthedocs.io/)
- [ ] [REDLog3G10](https://www.red.com/red-101/color-management-white-paper) - RED Log3G10 | [RED](https://www.red.com/), [colour](https://colour.readthedocs.io/)
- [ ] [Apple Log](https://developer.apple.com/documentation/avfoundation/avcapturecolorspace/applelog) - Apple ProRes Log (iPhone 15 Pro) | [Apple](https://developer.apple.com/), [colour](https://colour.readthedocs.io/)
- [ ] [Cineon](https://en.wikipedia.org/wiki/Cineon) - Kodak 1993 (film digitization log) | [wiki](https://en.wikipedia.org/wiki/Cineon), [colour](https://colour.readthedocs.io/)

#### Electro-Optical Transfer Functions

- [ ] [PQ (ST 2084)](https://en.wikipedia.org/wiki/Perceptual_quantizer) - Dolby/SMPTE perceptual quantizer | [spec](https://www.smpte.org/standards/st2084), [colour](https://colour.readthedocs.io/)
- [ ] [HLG (BT.2100)](https://en.wikipedia.org/wiki/Hybrid_log%E2%80%93gamma) - BBC/NHK hybrid log-gamma | [spec](https://www.itu.int/rec/R-REC-BT.2100), [colour](https://colour.readthedocs.io/)
- [ ] [sRGB EOTF](https://en.wikipedia.org/wiki/SRGB#Transfer_function_(%22gamma%22)) - IEC 61966-2-1 (piecewise gamma) | [spec](https://www.w3.org/Graphics/Color/srgb), [colour](https://colour.readthedocs.io/)
- [ ] [BT.709 OETF](https://en.wikipedia.org/wiki/Rec._709#Transfer_characteristics) - ITU-R BT.709 camera OETF | [spec](https://www.itu.int/rec/R-REC-BT.709), [colour](https://colour.readthedocs.io/)
- [ ] [BT.1886 EOTF](https://en.wikipedia.org/wiki/ITU-R_BT.1886) - ITU-R BT.1886 display EOTF | [spec](https://www.itu.int/rec/R-REC-BT.1886), [colour](https://colour.readthedocs.io/)

### Not Yet Implemented (Other)

- [ ] [LMS](https://en.wikipedia.org/wiki/LMS_color_space) - Cone response space (required for colorblindness simulation) | [impl](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html), [code](https://github.com/colour-science/colour/blob/develop/colour/models/lms.py)
- [ ] [YCoCg](https://en.wikipedia.org/wiki/YCoCg) - Fast luma-chroma for game/video compression | [impl](https://www.microsoft.com/en-us/research/publication/ycocg-r-a-color-space-with-rgb-reversibility/)
- [ ] [YIQ](https://en.wikipedia.org/wiki/YIQ) - NTSC analog color encoding | [impl](https://www.cs.rit.edu/~ncs/color/t_convert.html)
- [ ] [YDbDr](https://en.wikipedia.org/wiki/YDbDr) - SECAM analog color encoding | [impl](https://www.cs.rit.edu/~ncs/color/t_convert.html)
- [ ] [OkLCh](https://bottosson.github.io/posts/oklab/) - Cylindrical Oklab (hue, chroma, lightness) | [impl](https://bottosson.github.io/posts/oklab/), [code](https://www.w3.org/TR/css-color-4/#ok-lab)
- [ ] [IPT](https://en.wikipedia.org/wiki/IPT_color_space) - Hue-linear perceptual space (predecessor to ICtCp) | [impl](https://www.researchgate.net/publication/221677980_Development_and_Testing_of_a_Color_Space_IPT_with_Improved_Hue_Uniformity)
- [ ] [HSI](https://en.wikipedia.org/wiki/HSL_and_HSV#HSI_to_RGB) - Hue, Saturation, Intensity | [impl](https://www.cs.rit.edu/~ncs/color/t_convert.html)
- [ ] [TSL](https://en.wikipedia.org/wiki/TSL_color_space) - Tint, Saturation, Lightness | [paper](https://ieeexplore.ieee.org/document/840612), [code](https://github.com/colorjs/color-space/blob/822a488ad752326f85bd5491a853a4a5c32adc59/tsl.js)

## Quantization

### Tree-Based Methods

- [X] [Octree (OC)](https://www.codeproject.com/Articles/109133/Octree-Color-Palette) | [impl](https://rosettacode.org/wiki/Color_quantization#C.23), [code](https://github.com/cyotek/Cyotek.Drawing.Imaging.ColorReduction)

### Splitting Methods

- [X] [Median-cut (MC)](https://dl.acm.org/doi/10.1145/965139.807419) - Heckbert 1982 | [impl](https://www.leptonica.org/papers/mediancut.pdf), [code](https://github.com/nickkjolsing/median_cut)
- [X] [Variance-based method (WAN)](http://algorithmicbotany.org/papers/variance-based.pdf) - Wan et al. | [impl](http://algorithmicbotany.org/papers/variance-based.pdf)
- [X] [Binary splitting (BS)](https://opg.optica.org/josaa/fulltext.cfm?uri=josaa-11-11-2777&id=847) - Orchard & Bouman 1991 | [paper](https://opg.optica.org/josaa/fulltext.cfm?uri=josaa-11-11-2777&id=847)
- [X] [Binary splitting with Ant-tree (BSAT)](https://link.springer.com/article/10.1007/s11554-018-0814-8) - Pérez-Delgado 2018 | [paper](https://link.springer.com/article/10.1007/s11554-018-0814-8)
- [X] [Greedy orthogonal bi-partitioning (WU)](https://www.ece.mcmaster.ca/~xwu/cq.c) - Xiaolin Wu 1991 | [impl](https://www.ece.mcmaster.ca/~xwu/cq.c), [code](https://github.com/nickkjolsing/wu_color_quantization)
- [X] [Variance-cut (VC)](https://ieeexplore.ieee.org/document/6718239) - Celebi 2014 | [paper](https://ieeexplore.ieee.org/document/6718239)

### Clustering Methods

- [X] [K-Means](https://en.wikipedia.org/wiki/K-means_clustering) - Lloyd's algorithm for color clustering | [impl](https://stanford.edu/~cpiech/cs221/handouts/kmeans.html), [code](https://github.com/ibezkrovnyi/image-quantization/blob/master/src/palette/paletteQuantizer/pqKmeans.ts)
- [X] [Fuzzy C-Means](https://en.wikipedia.org/wiki/Fuzzy_clustering) - Soft clustering with membership degrees | [impl](https://www.sciencedirect.com/science/article/abs/pii/0098300484900207), [code](https://github.com/deric/clustering-benchmark)
- [X] [Incremental K-Means](https://en.wikipedia.org/wiki/K-means_clustering) - Online/streaming variant | [paper](https://www.cs.princeton.edu/courses/archive/fall08/cos436/Dwork/regress-online.pdf)

### Neural/Adaptive Methods

- [X] [Neuquant (NQ)](https://scientificgems.wordpress.com/stuff/neuquant-fast-high-quality-image-quantization/) - Dekker 1994 ([paper](https://dl.acm.org/doi/10.5555/225366.225374)) | [impl](https://scientificgems.wordpress.com/stuff/neuquant-fast-high-quality-image-quantization/), [code](https://github.com/nickkjolsing/neuquant)
- [X] [Adaptive distributing units (ADU)](https://www.tandfonline.com/doi/full/10.1179/1743131X13Y.0000000059) - Pérez-Delgado 2015 | [paper](https://www.tandfonline.com/doi/full/10.1179/1743131X13Y.0000000059)

### Hybrid/Advanced Methods

- [X] [WU combined with Ant-tree (ATCQ/WUATCQ)](https://github.com/mattdesl/atcq) - Pérez-Delgado ([paper](https://www.sciencedirect.com/science/article/abs/pii/S0031320314001526)) | [impl](https://github.com/mattdesl/atcq)
- [X] [BS combined with iterative ATCQ (BSITATCQ)](https://www.mdpi.com/2076-3417/10/21/7819) - Pérez-Delgado 2020 | [paper](https://www.mdpi.com/2076-3417/10/21/7819)
- [X] [Principal Component Analysis](https://en.wikipedia.org/wiki/Principal_component_analysis) - Hotelling 1933 | [impl](https://scikit-learn.org/stable/modules/generated/sklearn.decomposition.PCA.html), [tutorial](https://jakevdp.github.io/PythonDataScienceHandbook/05.09-principal-component-analysis.html)
- [X] [Spatial Color Quantization (SCQ)](https://dl.acm.org/doi/10.1145/1179352.1141943) - Puzicha et al. 2000 | [paper](https://dl.acm.org/doi/10.1145/1179352.1141943)

### Simple Methods

- [X] [Popularity](https://en.wikipedia.org/wiki/Color_quantization) - Frequency-based color selection | [impl](https://github.com/python-pillow/Pillow/blob/main/src/PIL/Image.py), [tutorial](https://pyimagesearch.com/2014/07/07/color-quantization-opencv-using-k-means-clustering/)
- [X] Fixed Palettes (EGA 16, VGA 256, Web-safe, Mac 8-bit) | [EGA](https://en.wikipedia.org/wiki/Enhanced_Graphics_Adapter), [VGA](https://en.wikipedia.org/wiki/VGA_color_palette), [websafe](https://en.wikipedia.org/wiki/Web_colors)

### Generic extensions

- [X] Ant-based optimization wrapper for any quantization algorithm | [ACO](https://en.wikipedia.org/wiki/Ant_colony_optimization_algorithms), [impl](https://github.com/Akavall/AntColonyOptimization)
- [X] PCA preprocessing wrapper for any quantization algorithm | [impl](https://scikit-learn.org/stable/modules/generated/sklearn.decomposition.PCA.html)

### Not Yet Implemented

- [ ] [Simulated Annealing](https://ieeexplore.ieee.org/document/6313077) - Bing et al. 2009 | [impl](https://github.com/perrygeo/simanneal), [tutorial](https://machinelearningmastery.com/simulated-annealing-from-scratch-in-python/)
- [ ] [BSDS300](https://www2.eecs.berkeley.edu/Research/Projects/CS/vision/bsds/) - Berkeley Segmentation Dataset | [dataset](https://www2.eecs.berkeley.edu/Research/Projects/CS/vision/bsds/)
- [ ] [Self-Organizing Maps (Kohonen)](https://en.wikipedia.org/wiki/Self-organizing_map) - Kohonen 1982 ([paper](https://link.springer.com/article/10.1007/BF00337288)) | [impl](https://github.com/JustGlowing/minisom), [tutorial](https://towardsdatascience.com/self-organizing-maps-1b7d2a84e065)
- [ ] [Modified Median Cut (MMCQ)](https://www.leptonica.org/color-quantization.html) - Bloomberg (Leptonica) | [impl](https://github.com/lokesh/color-thief), [Leptonica](https://www.leptonica.org/color-quantization.html)

## Color Distance Metrics

### RGB-Based Distances

- [X] [Euclidean BT.709](https://en.wikipedia.org/wiki/Rec._709) - ITU-R BT.709 luma weights | [spec](https://www.itu.int/rec/R-REC-BT.709), [impl](https://github.com/ibezkrovnyi/image-quantization)
- [X] [Manhattan BT.709](https://en.wikipedia.org/wiki/Taxicab_geometry) - L1 norm with BT.709 weights | [wiki](https://en.wikipedia.org/wiki/Taxicab_geometry)
- [X] [Euclidean Nommyde](https://github.com/ibezkrovnyi/image-quantization) - Alternative weighting | [impl](https://github.com/ibezkrovnyi/image-quantization)
- [X] [Manhattan Nommyde](https://github.com/ibezkrovnyi/image-quantization) - L1 with Nommyde weights | [impl](https://github.com/ibezkrovnyi/image-quantization)
- [X] Euclidean LowRed / HighRed variants - Adaptive red weighting | [color-metric](https://www.compuphase.com/cmetric.htm)
- [X] Manhattan LowRed / HighRed variants - L1 with adaptive red | [color-metric](https://www.compuphase.com/cmetric.htm)
- [X] [CompuPhase Redmean](https://www.compuphase.com/cmetric.htm) - Thiadmer Riemersma (perceptual approximation) | [article](https://www.compuphase.com/cmetric.htm)
- [X] [PNGQuant](https://github.com/pornel/pngquant) - Kornel Lesiński (alpha-aware) | [impl](https://github.com/pornel/pngquant), [libimagequant](https://github.com/ImageOptim/libimagequant)

### CIE-Based Distances

- [X] [CIE76 (ΔE*ab)](https://en.wikipedia.org/wiki/Color_difference#CIE76) - Euclidean in Lab (1976) | [impl](http://www.brucelindbloom.com/index.html?Eqn_DeltaE_CIE76.html), [colormine](https://github.com/colormine/colormine)
- [X] [CIE94](https://en.wikipedia.org/wiki/Color_difference#CIE94) - Textile/Graphic Arts variants (1994) | [impl](http://www.brucelindbloom.com/index.html?Eqn_DeltaE_CIE94.html), [colormath](https://python-colormath.readthedocs.io/)
- [X] [CIEDE2000](https://en.wikipedia.org/wiki/Color_difference#CIEDE2000) - Most accurate Lab distance ([paper](http://www2.ece.rochester.edu/~gsharma/ciede2000/ciede2000noteCRNA.pdf)) | [impl](http://www2.ece.rochester.edu/~gsharma/ciede2000/), [colormath](https://python-colormath.readthedocs.io/)
- [X] [CMC l:c](https://en.wikipedia.org/wiki/Color_difference#CMC_l:c_(1984)) - Clarke et al. 1984 (Perceptibility l=1,c=1 / Acceptability l=2,c=1) | [impl](http://www.brucelindbloom.com/index.html?Eqn_DeltaE_CMC.html)
- [X] [DIN99](https://de.wikipedia.org/wiki/DIN99-Farbraum) - German industrial standard 2001 | [paper](https://www.beuth.de/de/norm/din-6176/4238073)

### Modern Perceptual Distances

- [X] [Oklab](https://bottosson.github.io/posts/oklab/) - Ottosson 2020 (Euclidean in Oklab) | [impl](https://bottosson.github.io/posts/oklab/), [colour](https://colour.readthedocs.io/)
- [X] [JzAzBz](https://www.osapublishing.org/oe/fulltext.cfm?uri=oe-25-13-15131) - Safdar et al. 2017 (HDR-capable) | [impl](https://observablehq.com/@jrus/jzazbz), [colour](https://colour.readthedocs.io/)
- [X] [ICtCp](https://en.wikipedia.org/wiki/ICtCp) - Dolby/ITU-R BT.2100 (HDR perceptual) | [spec](https://www.itu.int/rec/R-REC-BT.2100), [colour](https://colour.readthedocs.io/)

### Generic Color Space Distances

- [X] `EuclideanDistance<TColorSpace>` - Generic Euclidean in any 3/4-component color space | [math](https://en.wikipedia.org/wiki/Euclidean_distance)
- [X] `EuclideanDistanceSquared<TColorSpace>` - Faster variant for comparisons only
- [X] `WeightedEuclideanDistance<TColorSpace>` - Generic Weighted Euclidean in any 3/4-component color space
- [X] `WeightedEuclideanDistanceSquared<TColorSpace>` - Faster variant for comparisons only
- [X] `ManhattanDistance<TColorSpace>` - Generic Manhattan in any 3/4-component color space | [math](https://en.wikipedia.org/wiki/Taxicab_geometry)
- [X] `ManhattanDistanceSquared<TColorSpace>` - Faster variant for comparisons only
- [X] `WeightedManhattanDistance<TColorSpace>` - Generic Weighted Manhattan in any 3/4-component color space
- [X] `WeightedManhattanDistanceSquared<TColorSpace>` - Faster variant for comparisons only
- [X] `ChebyshevDistance<TColorSpace>` - Generic Chebyshev in any 3/4-component color space | [math](https://en.wikipedia.org/wiki/Chebyshev_distance)
- [X] `ChebyshevDistanceSquared<TColorSpace>` - Faster variant for comparisons only
- [X] `WeightedChebyshevDistance<TColorSpace>` - Generic Weighted Chebyshev in any 3/4-component color space
- [X] `WeightedChebyshevDistanceSquared<TColorSpace>` - Faster variant for comparisons only

### Not Yet Implemented

- [ ] [Linear RGB](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/) | [impl](https://github.com/matejlou/tetrapal)
- [ ] [CAM02-UCS ΔE](https://doi.org/10.1002/col.20227) - Luo et al. 2006 (uniform from CIECAM02) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_CAM02UCS.html), [paper](https://doi.org/10.1002/col.20227)
- [ ] [CAM02-LCD ΔE](https://doi.org/10.1002/col.20227) - Large Color Difference variant | [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_CAM02LCD.html)
- [ ] [CAM02-SCD ΔE](https://doi.org/10.1002/col.20227) - Small Color Difference variant | [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_CAM02SCD.html)
- [ ] [CAM16-UCS ΔE](https://en.wikipedia.org/wiki/Color_appearance_model#CIECAM16) - Li et al. 2017 (most perceptually accurate) | [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_CAM16UCS.html), [paper](https://doi.org/10.1002/col.22131)
- [ ] [CAM16-LCD ΔE](https://doi.org/10.1002/col.22131) - Large Color Difference variant | [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_CAM16LCD.html)
- [ ] [CAM16-SCD ΔE](https://doi.org/10.1002/col.22131) - Small Color Difference variant | [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_CAM16SCD.html)
- [ ] [HyAB](https://doi.org/10.1007/978-3-030-26656-2_1) - Abasi et al. 2019 (hybrid Lab + chroma) | [paper](https://doi.org/10.1007/978-3-030-26656-2_1), [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_HyAB.html)
- [ ] [HyCH](https://doi.org/10.1007/978-3-030-26656-2_1) - Hybrid Chroma-Hue variant | [colour](https://colour.readthedocs.io/)
- [ ] [ΔE ITP](https://www.itu.int/rec/R-REC-BT.2124) - ITU-R BT.2124 (ICtCp color difference) | [spec](https://www.itu.int/rec/R-REC-BT.2124), [colour](https://colour.readthedocs.io/en/latest/generated/colour.difference.delta_E_ITP.html)
- [ ] [DIN99 ΔE (variants)](https://doi.org/10.1002/col.20222) - DIN99b/c/d color differences | [paper](https://doi.org/10.1002/col.20222), [colour](https://colour.readthedocs.io/)
- [ ] [Wasserstein Distance](https://en.wikipedia.org/wiki/Wasserstein_metric) - Earth Mover's Distance for palette comparison | [impl](https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.wasserstein_distance.html), [POT](https://pythonot.github.io/)

## Dithering

### References

- [Dithering Matrices](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html)
- [DitherPunk](https://surma.dev/things/ditherpunk/)
- [Cyotek](https://github.com/cyotek/Dithering/tree/master/src/Dithering)
- [Tetrapal](https://github.com/matejlou/tetrapal)
- [Dithermark](https://dithermark.com/resources/)

### Error Diffusion

- [X] [Floyd-Steinberg](https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering) - Floyd & Steinberg 1976 ([paper](https://dl.acm.org/doi/10.1145/360349.360351))
- [X] [False Floyd-Steinberg](https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering) - Simplified variant
- [X] [Floyd-Steinberg (equally distributed)](https://github.com/kgjenkins/dither-dream) - Modified weights | [impl](https://github.com/kgjenkins/dither-dream)
- [X] [Jarvis, Judice, and Ninke](https://dl.acm.org/doi/10.1145/360349.360351) - JJN 1976 | [impl](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html), [Cyotek](https://github.com/cyotek/Dithering)
- [X] [Stucki](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html) - Peter Stucki 1981 | [impl](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html)
- [X] [Atkinson](https://en.wikipedia.org/wiki/Atkinson_dithering) - Bill Atkinson (Apple) | [impl](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html), [Cyotek](https://github.com/cyotek/Dithering)
- [X] [Burkes](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html) - Daniel Burkes 1988 | [impl](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html)
- [X] [Sierra / Two-Row Sierra / Sierra Lite](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html) - Frankie Sierra 1989/1990 | [impl](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html)
- [X] [Pigeon](https://hbfs.wordpress.com/2013/12/31/dithering/) - Pigeon 2013 | [article](https://hbfs.wordpress.com/2013/12/31/dithering/)
- [X] [Stevenson-Arce](https://ieeexplore.ieee.org/document/1094973) - Stevenson & Arce 1985 | [paper](https://ieeexplore.ieee.org/document/1094973)
- [X] [Fan](https://ditherit.com) / [ShiauFan](https://ieeexplore.ieee.org/document/476827) / [ShiauFan2](https://ieeexplore.ieee.org/document/476827) - Shiau & Fan 1996 | [paper](https://ieeexplore.ieee.org/document/476827)
- [X] [TwoD / Down / DoubleDown / Diagonal](https://github.com/sehugg/dithertron) - Dithertron variants | [impl](https://github.com/sehugg/dithertron)
- [X] [VerticalDiamond / HorizontalDiamond / Diamond](https://github.com/sehugg/dithertron) - Dithertron variants | [impl](https://github.com/sehugg/dithertron)

### Matrix-based (Ordered)

- [X] [Bayer Matrix](https://en.wikipedia.org/wiki/Ordered_dithering) - Bayer 1973 (2x2 to arbitrary 2^n sizes) | [impl](https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html), [surma](https://surma.dev/things/ditherpunk/)
- [X] [Halftone](https://en.wikipedia.org/wiki/Halftone) - Screening patterns | [impl](https://github.com/cyotek/Dithering), [wiki](https://en.wikipedia.org/wiki/Halftone)
- [X] [Interleaved Gradient Noise](https://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare/) - Jimenez 2014 (SIGGRAPH) | [slides](https://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare/)
- [X] [Cluster Dot](https://en.wikipedia.org/wiki/Halftone#Clustered-dot_ordered_dithering) - 3x3, 4x4, 8x8 variants | [wiki](https://en.wikipedia.org/wiki/Halftone#Clustered-dot_ordered_dithering)

### Arithmetic Dither

[Reference](https://pippin.gimp.org/a_dither/) - Procedural spatial dithering

- [X] XOR-Y149 pattern / with channel variation | [ref](https://pippin.gimp.org/a_dither/)
- [X] XY Arithmetic pattern / with channel variation | [ref](https://pippin.gimp.org/a_dither/)
- [X] Uniform pattern | [ref](https://pippin.gimp.org/a_dither/)

### Space-Filling Curve Dithering

- [X] [Riemersma](https://www.compuphase.com/riemer.htm) - Thiadmer Riemersma (Hilbert curve variants) | [article](https://www.compuphase.com/riemer.htm), [code](https://www.compuphase.com/riemersma.c)

### Noise-Based Dithering

- [X] [White Noise](https://en.wikipedia.org/wiki/White_noise) - Random uniform distribution (Light/Normal/Strong) | [wiki](https://en.wikipedia.org/wiki/White_noise)
- [X] [Blue Noise](https://blog.demofox.org/2018/01/30/what-the-heck-is-blue-noise/) - Ulichney 1988 (high-freq, low clumping) | [article](https://blog.demofox.org/2018/01/30/what-the-heck-is-blue-noise/), [impl](https://momentsingraphics.de/BlueNoise.html)
- [X] [Brown Noise](https://en.wikipedia.org/wiki/Brownian_noise) - Random walk (1/f² spectrum) | [wiki](https://en.wikipedia.org/wiki/Brownian_noise)
- [X] [Pink Noise](https://en.wikipedia.org/wiki/Pink_noise) - 1/f spectrum (equal power per octave) | [wiki](https://en.wikipedia.org/wiki/Pink_noise), [impl](https://www.firstpr.com.au/dsp/pink-noise/)

### Advanced Dithering Algorithms

- [X] [Average](https://www.graphicsacademy.com/what_dithera.php) - Mean color quantization | [ref](https://www.graphicsacademy.com/what_dithera.php)
- [X] [Random](https://www.graphicsacademy.com/what_ditherr.php) - Stochastic noise dithering | [ref](https://www.graphicsacademy.com/what_ditherr.php)
- [X] [Void and Cluster](https://ieeexplore.ieee.org/document/241109) - Ulichney 1993 | [paper](https://ieeexplore.ieee.org/document/241109), [impl](https://blog.demofox.org/2019/06/25/generating-blue-noise-textures-with-void-and-cluster/)
- [X] [Gradient-Aware](https://surma.dev/things/ditherpunk/) - Preserves image gradients | [article](https://surma.dev/things/ditherpunk/)
- [X] [Debanding](https://shader-tutorial.dev/advanced/color-banding-dithering/) - Reduces posterization artifacts | [tutorial](https://shader-tutorial.dev/advanced/color-banding-dithering/)
- [X] [Joel Yliluoma's algorithms 1, 2, 3](https://bisqwit.iki.fi/story/howto/dither/jy/) - Bisqwit 2011 (Algorithm 3 includes full iterative subdivision) | [article](https://bisqwit.iki.fi/story/howto/dither/jy/)
- [X] [Thomas Knoll](https://helpx.adobe.com/photoshop/using/converting-color-modes.html) - Photoshop pattern dither (4 variants) | [ref](https://helpx.adobe.com/photoshop/using/converting-color-modes.html)
- [X] [N-Closest](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/) - Matěj Loužecký 2023 | [article](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/), [impl](https://github.com/matejlou/tetrapal)
- [X] [N-Convex](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/) - Convex hull color mixing | [article](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/), [impl](https://github.com/matejlou/tetrapal)
- [X] [Ostromoukhov Variable-Coefficient](https://perso.liris.cnrs.fr/victor.ostromoukhov/publications/pdf/SIGGRAPH01_varcoeff.pdf) - Ostromoukhov 2001 (SIGGRAPH) | [paper](https://perso.liris.cnrs.fr/victor.ostromoukhov/publications/pdf/SIGGRAPH01_varcoeff.pdf), [code](https://perso.liris.cnrs.fr/victor.ostromoukhov/research.html)
- [X] [Dizzy Dithering](https://liamappelbe.medium.com/dizzy-dithering-2ae76dbceba1) - Liam Appelbe 2024 | [article](https://liamappelbe.medium.com/dizzy-dithering-2ae76dbceba1)

### Structure/Content-Aware

- [X] Structure-Aware Error Diffusion (Default/Priority/Large radius) | [concept](https://ieeexplore.ieee.org/document/1407720)
- [X] Adaptive Dithering (Quality/Balanced/Performance/Smart ML-based selection)

### Scanning Modes

- [X] Serpentine/Boustrophedon - flag with any matrix-based ditherer | [wiki](https://en.wikipedia.org/wiki/Boustrophedon)

### Geometric/Interpolation Dithering

- [X] [Barycentric](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/) - Triangle-based color interpolation | [article](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/), [impl](https://github.com/matejlou/tetrapal)
- [X] [Triangulated Irregular Network (TIN)](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/) - Delaunay tessellation dithering | [article](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/)
- [X] [Natural Neighbour](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/) - Voronoi-based area-weighted interpolation | [article](https://matejlou.blog/2023/12/06/ordered-dithering-for-arbitrary-or-irregular-palettes/)
- [X] [Direct Binary Search (DBS)](https://ieeexplore.ieee.org/document/413596) - Iterative HVS-based halftone optimization | [paper](https://ieeexplore.ieee.org/document/413596)

## Image Upscaling

### References

- <http://www.datagenetics.com/blog/december32013/index.html>
- <https://github.com/janert/pixelscalers>
- <https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms>
- <https://github.com/bbbbbr/gimp-plugin-pixel-art-scalers>
- <https://github.com/Dnyarri/PixelArtScaling/tree/main/scalenx>
- <https://emulation.gametechwiki.com/index.php/Shaders,_filters,_and_sound>

### Emulator Source References

| Emulator | Filters Location | Notable Scalers |
|----------|------------------|-----------------|
| [Snes9x](https://github.com/snes9xgit/snes9x/tree/master/filter) | `filter/` | 2xSaI, EPX, HQ2x, xBRZ, snes_ntsc, Sharp-Bilinear |
| [RetroArch/LibRetro](https://github.com/libretro/slang-shaders) | `slang-shaders/` | CRT, xBR, ScaleFX, DDT, SABR, NTSC, Handheld |
| [MAME](https://github.com/mamedev/mame) | `src/osd/modules/render/` | AdvInterp, TV2x/3x, RGB2x/3x, Scan2x/3x |
| [DOSBox](https://www.dosbox.com/wiki/Scaler) | config `scaler=` | normal2x/3x, advmame2x/3x, hq2x/3x, 2xsai, super2xsai, advinterp2x/3x, tv2x/3x, rgb2x/3x, scan2x/3x |
| [VBA-M](https://github.com/visualboyadvance-m/visualboyadvance-m) | `src/filters/` | HQ2x-4x, xBRZ, BilinearPlus, TV Mode, Scanlines |
| [ZSNES](https://zsnes.com/) | built-in | Eagle, Super Eagle, 2xSaI, Super 2xSaI, HQ2x, NTSC |
| [FCEUX](https://github.com/TASEmulators/fceux) | `src/drivers/` | NTSC, HQ2x-4x, Scale2x/3x, Prescale |
| [Kega Fusion](https://segaretro.org/Kega_Fusion) | built-in | HQ2x, 2xSaI, Scale2x, NTSC |
| [Genesis Plus GX](https://github.com/ekeeke/Genesis-Plus-GX) | built-in | Bilinear, HQ2x, NTSC (blargg) |
| [NO$GBA / NO$GMB](https://problemkaputt.de/) | built-in | 2xSaI, Super Eagle |
| [Nestopia](https://github.com/0ldsk00l/nestopia) | built-in | NTSC (blargg), Scanlines |
| [Mednafen](https://mednafen.github.io/) | built-in | HQ2x-4x, Scale2x/3x, NTSC, CRT |
| [BizHawk](https://github.com/TASEmulators/BizHawk) | `src/BizHawk.Client.EmuHawk/` | HQ2x-4x, Scanlines |
| [Altirra](https://virtualdub.org/altirra.html) | built-in | Bicubic, Sharp Bilinear, Scanlines |
| [FS-UAE](https://fs-uae.net/) | built-in | Scanlines, Shader support |
| [ScummVM](https://www.scummvm.org/) | built-in | HQ2x/3x, Edge, AdvMAME, TV2x/3x, DotMatrix |
| [Desmume](https://desmume.org/) | built-in | HQ2x-4x, Scanlines, 2xSaI |
| [PCSX2](https://pcsx2.net/) | built-in | FXAA, CRT shaders, xBR |
| [Dolphin](https://dolphin-emu.org/) | built-in | Anisotropic, AA, PostFX shaders |

### Classic Pixel Art Scalers

- [X] [Scale2x/3x/4x](https://www.scale2x.it/) - Andrea Mazzoleni 2001 ([AdvMAME](https://www.mamedev.org/)) | [impl](https://www.scale2x.it/), [code](https://github.com/amadvance/scale2x)
- [X] [Scale2xPlus/3xPlus](https://www.scale2x.it/) - With subpixel interpolation | [impl](https://www.scale2x.it/)
- [X] [Eagle2x/3x/3xB](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#Eagle) - Kreed ~1990s (ZSNES) | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#Eagle)
- [X] [Super Eagle](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#Super_Eagle) - Kreed | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#Super_Eagle)
- [X] [EPX/EPXB/EPX3/EPXC](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#EPX/Scale2%C3%97/AdvMAME2%C3%97) - SNES9x (Eric Johnston 1992) | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#EPX)
- [X] [DES/DES2x](https://www.romhacking.net/) - FNES | [ref](https://www.romhacking.net/)
- [X] [SCL2x/SuperSCL2x/UltraSCL2x](https://www.romhacking.net/) - FNES | [ref](https://www.romhacking.net/)

### HQ/LQ Family

- [X] [HQ2x/3x/4x](https://en.wikipedia.org/wiki/Hqx) - Maxim Stepin 2003 ([homepage](http://www.hiend3d.com/hq2x.html)) | [impl](https://github.com/grom358/hqx), [wiki](https://en.wikipedia.org/wiki/Hqx)
- [X] [LQ2x/3x/4x](https://en.wikipedia.org/wiki/Hqx) - Faster/simplified variants | [wiki](https://en.wikipedia.org/wiki/Hqx)
- [X] HQ2x/3x/4x Bold/Smart variants - Community improvements | [libretro](https://github.com/libretro/common-shaders)

### XBR Family

- [X] [XBR2x/3x/4x/5x](https://forums.libretro.com/t/xbr-algorithm-tutorial/123) - Hyllian 2011 ([tutorial](https://forums.libretro.com/t/xbr-algorithm-tutorial/123)) | [impl](https://github.com/libretro/common-shaders/tree/master/xbr)
- [X] [XBRz 2x-6x](https://sourceforge.net/projects/xbrz/) - Zenju 2012 (optimized C++) | [impl](https://sourceforge.net/projects/xbrz/)
- [X] [Super xBR](https://github.com/libretro/common-shaders/tree/master/xbr/shaders/super-xbr) - Enhanced for photos | [impl](https://github.com/libretro/common-shaders/tree/master/xbr/shaders/super-xbr)

### SaI Family

- [X] [2xSaI](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2%C3%97SaI) (Derek Liauw Kie Fa) | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2%C3%97SaI), [impl](https://github.com/libretro/common-shaders)
- [X] [Super 2xSaI](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2%C3%97SaI) | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#2%C3%97SaI)
- [X] [2xSaL](https://forums.libretro.com/t/shaders-related-to-image-upscaling/3838) (guest(r)) - Level 1/2 | [ref](https://forums.libretro.com/t/shaders-related-to-image-upscaling/3838)

### Advanced Pixel Art Scalers

- [X] [SABR v3.0](https://github.com/libretro/common-shaders/tree/master/sabr) - Joshua Street (multi-directional edge detection) | [impl](https://github.com/libretro/common-shaders/tree/master/sabr)
- [X] [ScaleFX](https://github.com/libretro/common-shaders/tree/master/scalefx) - Sp00kyFox (up to level 6) | [impl](https://github.com/libretro/common-shaders/tree/master/scalefx)
- [X] [DDT](https://github.com/libretro/common-shaders/tree/master/ddt) - Hyllian (Data-Dependent Triangulation) | [impl](https://github.com/libretro/common-shaders/tree/master/ddt)
- [X] [DDT-Extended](https://github.com/libretro/common-shaders/tree/master/ddt) - 16-pixel voting variant | [impl](https://github.com/libretro/common-shaders/tree/master/ddt)
- [X] [DDT-Sharp](https://github.com/libretro/common-shaders/tree/master/ddt) - Weighted blending variant | [impl](https://github.com/libretro/common-shaders/tree/master/ddt)
- [X] [MMPX](https://casual-effects.com/research/McGuire2021PixelArt/index.html) - McGuire & Gagiu 2021 ([paper](https://casual-effects.com/research/McGuire2021PixelArt/McGuire2021_MMPX.pdf)) | [impl](https://casual-effects.com/research/McGuire2021PixelArt/index.html)
- [X] [RotSprite](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#RotSprite) - Xenowhirl 2007 (rotation-aware) | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#RotSprite)
- [X] [Quilez](https://iquilezles.org/articles/texture/) - Íñigo Quílez (smooth texture filtering) | [article](https://iquilezles.org/articles/texture/)

### MAME/Emulator Scalers

- [X] [AdvInterp2x/3x/4x](https://www.mamedev.org/) (MAME) | [MAME](https://github.com/mamedev/mame)
- [X] [TV2x/3x/4x](https://www.mamedev.org/) - Scanline simulation | [MAME](https://github.com/mamedev/mame)
- [X] [RGB2x/3x/4x](https://www.mamedev.org/) - Phosphor simulation | [MAME](https://github.com/mamedev/mame)
- [X] [Scan2x/3x/4x](https://www.mamedev.org/) - Scanline effects | [MAME](https://github.com/mamedev/mame)

### CRT/Retro Effects

- [X] [NTSC Simulation](https://wiki.nesdev.org/w/index.php/NTSC_video) (blargg) - Composite/S-Video/RGB/Mono | [impl](https://github.com/blargg), [wiki](https://wiki.nesdev.org/w/index.php/NTSC_video)
- [X] [2xPM](https://github.com/libretro/common-shaders) (Pablo Medina) - Pixel morphing | [impl](https://github.com/libretro/common-shaders)
- [X] CRT/Scanline effects - Various modes | [libretro](https://github.com/libretro/common-shaders/tree/master/crt)

### Not Yet Implemented - CRT Shaders

- [ ] [CRT-Geom](https://github.com/libretro/slang-shaders/tree/master/crt) - cgwg (configurable aperture grille) | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [CRT-Royale](https://github.com/libretro/slang-shaders/blob/master/crt/crt-royale.slangp) - TroggleMonkey (most advanced CRT) | [impl](https://github.com/libretro/slang-shaders/blob/master/crt/crt-royale.slangp)
- [ ] [CRT-Lottes](https://github.com/libretro/slang-shaders/tree/master/crt) - Timothy Lottes (slot mask + bloom) | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [CRT-Hyllian](https://github.com/libretro/slang-shaders/tree/master/crt) - Hyllian (lightweight, efficient) | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [CRT-Guest-Advanced](https://forums.libretro.com/t/new-crt-shader-from-guest-crt-guest-advanced-updates/25444) - guest.r (feature-rich) | [impl](https://github.com/libretro/slang-shaders/tree/master/crt), [thread](https://forums.libretro.com/t/new-crt-shader-from-guest-crt-guest-advanced-updates/25444)
- [ ] [CRT-Easymode](https://github.com/libretro/slang-shaders/tree/master/crt) - Lightweight beginner-friendly | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [CRT-Caligari](https://github.com/libretro/slang-shaders/tree/master/crt) - Alternative curvature method | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [zfast-CRT](https://github.com/libretro/slang-shaders/tree/master/crt) - Extremely fast (Raspberry Pi) | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [GTU](https://github.com/libretro/slang-shaders/tree/master/crt) - aliaspider (signal bandwidth simulation) | [impl](https://github.com/libretro/slang-shaders/tree/master/crt)
- [ ] [GTU-Famicom](https://github.com/libretro/common-shaders/tree/master/crt/shaders/GTU-famicom) - aliaspider (NES/Famicom NTSC PPU) | [impl](https://github.com/libretro/common-shaders/tree/master/crt/shaders/GTU-famicom)
- [ ] [Sony Megatron](https://forums.libretro.com/t/sony-megatron-colour-video-monitor/36109) - HDR CRT shader | [thread](https://forums.libretro.com/t/sony-megatron-colour-video-monitor/36109)
- [ ] [NTSC-CRT](https://github.com/LMP88959/NTSC-CRT) - LMP88959 (EMMIR) accurate NTSC | [impl](https://github.com/LMP88959/NTSC-CRT)
- [ ] [Mega Bezel](https://forums.libretro.com/t/mega-bezel-reflection-shader-feedback-and-updates/25512) - HyperspaceMadness (bezel reflections) | [thread](https://forums.libretro.com/t/mega-bezel-reflection-shader-feedback-and-updates/25512)
- [ ] [Koko-aio](https://forums.libretro.com/t/koko-aio-shader-discussions-and-updates/38455) - kokoko3k (all-in-one CRT) | [thread](https://forums.libretro.com/t/koko-aio-shader-discussions-and-updates/38455)
- [ ] [tvout-tweaks](https://github.com/libretro/common-shaders/blob/master/crt/shaders/tvout-tweaks.cg) - aliaspider (TV output simulation) | [impl](https://github.com/libretro/common-shaders/blob/master/crt/shaders/tvout-tweaks.cg)

### Not Yet Implemented - NTSC/PAL Video Simulation

- [ ] [blargg NTSC](http://slack.net/~ant/libs/ntsc.html) - System-specific NTSC (NES/SNES/SMS/Genesis) | [lib](http://slack.net/~ant/libs/ntsc.html), [impl](https://github.com/libretro/bsnes-libretro)
- [ ] [Maister NTSC](https://github.com/libretro/common-shaders/tree/master/ntsc) - Generic NTSC shader (256px/320px presets) | [impl](https://github.com/libretro/common-shaders/tree/master/ntsc)
- [ ] [PAL-R57 Shell](https://github.com/libretro/slang-shaders/tree/master/pal) - PAL color encoding simulation | [impl](https://github.com/libretro/slang-shaders/tree/master/pal)
- [ ] [Composite Direct](https://github.com/libretro/slang-shaders/tree/master/ntsc) - S-Video/Composite simulation | [impl](https://github.com/libretro/slang-shaders/tree/master/ntsc)

### Anti-Aliasing Scalers

- [X] [FXAA](https://developer.download.nvidia.com/assets/gamedev/files/sdk/11/FXAA_WhitePaper.pdf) - Timothy Lottes 2009 (NVIDIA) | [paper](https://developer.download.nvidia.com/assets/gamedev/files/sdk/11/FXAA_WhitePaper.pdf), [impl](https://github.com/libretro/common-shaders/tree/master/anti-aliasing/shaders/fxaa)
- [X] [Advanced-AA](https://forums.libretro.com/t/shaders-related-to-image-upscaling/3838) - guest(r) 2006 | [ref](https://forums.libretro.com/t/shaders-related-to-image-upscaling/3838)
- [X] [Reverse-AA](https://github.com/Wikipedia-Gfx/aaimage) - Christoph Feck 2012 | [impl](https://github.com/Wikipedia-Gfx/aaimage)
- [X] [AANN](https://github.com/Wikipedia-Gfx/aaimage) - Anti-aliased nearest neighbor | [impl](https://github.com/Wikipedia-Gfx/aaimage)
- [X] [SMAA](https://www.iryoku.com/smaa/) - Jimenez et al. 2012 (Subpixel Morphological AA) | [project](https://www.iryoku.com/smaa/), [impl](https://github.com/iryoku/smaa)
- [X] [MLAA](https://www.iryoku.com/mlaa/) - Reshetov 2009 (Morphological AA) | [project](https://www.iryoku.com/mlaa/)
- [ ] [TAA](https://en.wikipedia.org/wiki/Temporal_anti-aliasing) - Temporal Anti-Aliasing | [wiki](https://en.wikipedia.org/wiki/Temporal_anti-aliasing)

### Interpolation Scalers

- [X] [Bicubic](https://en.wikipedia.org/wiki/Bicubic_interpolation) - Keys 1981 (Mitchell-Netravali variants) | [wiki](https://en.wikipedia.org/wiki/Bicubic_interpolation), [impl](https://www.paulinternet.nl/?page=bicubic)
- [X] [Lanczos 2/3/4](https://en.wikipedia.org/wiki/Lanczos_resampling) - Lanczos 1950s (windowed sinc) | [wiki](https://en.wikipedia.org/wiki/Lanczos_resampling)
- [X] [Catmull-Rom Spline](https://en.wikipedia.org/wiki/Cubic_Hermite_spline#Catmull%E2%80%93Rom_spline) - Catmull & Rom 1974 (B=0, C=0.5) | [wiki](https://en.wikipedia.org/wiki/Catmull%E2%80%93Rom_spline)
- [X] [B-Spline](https://en.wikipedia.org/wiki/B-spline) - Schoenberg 1946 (B=1, C=0) | [wiki](https://en.wikipedia.org/wiki/B-spline)
- [X] [Mitchell-Netravali](https://dl.acm.org/doi/10.1145/54852.378514) - Mitchell & Netravali 1988 (B=1/3, C=1/3) | [paper](https://dl.acm.org/doi/10.1145/54852.378514), [impl](http://www.imagemagick.org/Usage/filter/)
- [X] [Robidoux](https://www.imagemagick.org/discourse-server/viewtopic.php?t=15319) - Nicolas Robidoux (B=0.3782, C=0.3109) optimized Mitchell-Netravali variant | [imagemagick](https://www.imagemagick.org/discourse-server/viewtopic.php?t=15319)
- [X] [RobidouxSharp](https://www.imagemagick.org/discourse-server/viewtopic.php?t=15319) - Nicolas Robidoux (B=0.2620, C=0.3690) sharper variant | [imagemagick](https://www.imagemagick.org/discourse-server/viewtopic.php?t=15319)
- [X] [Spline16/36/64](http://www.antigrain.com/research/bicubic_interpolation/) - Anti-Grain Geometry higher-degree polynomial splines (4x4, 6x6, 8x8 kernels) | [agg](http://www.antigrain.com/research/bicubic_interpolation/)
- [X] [Smoothstep](https://en.wikipedia.org/wiki/Smoothstep) - Hermite interpolation variants | [wiki](https://en.wikipedia.org/wiki/Smoothstep)
- [X] [BilinearPlus/BilinearPlusFaster](https://github.com/visualboyadvance-m/visualboyadvance-m) - VBA enhanced bilinear | [impl](https://github.com/visualboyadvance-m/visualboyadvance-m)

### Edge-Directed Scalers

- [X] [NEDI](https://ieeexplore.ieee.org/document/1284395) - Li & Orchard 2001 (New Edge-Directed Interpolation) | [paper](https://ieeexplore.ieee.org/document/1284395)
- [X] [Bilateral Filter](https://en.wikipedia.org/wiki/Bilateral_filter) - Tomasi & Manduchi 1998 ([paper](https://users.soe.ucsc.edu/~manduchi/Papers/ICCV98.pdf)) | [impl](https://docs.opencv.org/master/d4/d86/group__imgproc__filter.html#ga9d7064d478c95d60003cf839430737ed)

### Other Scalers

- [X] [4xSoft Smart](https://forums.libretro.com/t/shaders-related-to-image-upscaling/3838) - guest(r) 2016 | [ref](https://forums.libretro.com/t/shaders-related-to-image-upscaling/3838)
- [X] [ScaleHQ 2x/4x](https://github.com/libretro/common-shaders/tree/master/scalehq) - LibRetro | [impl](https://github.com/libretro/common-shaders/tree/master/scalehq)
- [X] [Seam Carving](https://en.wikipedia.org/wiki/Seam_carving) - Avidan & Shamir 2007 ([paper](https://perso.crans.org/frenoy/2IMG/SeamCarving.pdf)) | [impl](https://github.com/esimov/caire), [demo](https://zulko.github.io/blog/2014/05/24/avidan-shamir-seam-carving/)

### Not Yet Implemented - Pixel Art

- [X] [SAA5050 Diagonal Smoothing](https://en.wikipedia.org/wiki/Mullard_SAA5050) - Mullard 1980 (Teletext character smoothing) | [wiki](https://en.wikipedia.org/wiki/Mullard_SAA5050), [MAME](https://github.com/mamedev/mame/blob/master/src/devices/video/saa5050.cpp)
- [X] [Scale2xSFX/3xSFX](https://web.archive.org/web/20160527015550/https://libretro.com/forums/archive/index.php?t-1655.html) - ScaleFX combined with ScaleNx | [ref](https://web.archive.org/web/20160527015550/https://libretro.com/forums/archive/index.php?t-1655.html)
- [X] [Kopf-Lischinski](https://johanneskopf.de/publications/pixelart/paper/pixel.pdf) - Depixelizing pixel art (vectorization) | [paper](https://johanneskopf.de/publications/pixelart/paper/pixel.pdf), [project](https://johanneskopf.de/publications/pixelart/)
- [X] [Edge Scaler](https://wiki.scummvm.org/index.php/Scalers) - ScummVM simple edge enhancement | [scummvm](https://wiki.scummvm.org/index.php/Scalers)
- [X] [DotMatrix](https://wiki.scummvm.org/index.php/Scalers) - ScummVM simulated dot-matrix display | [scummvm](https://wiki.scummvm.org/index.php/Scalers)
- [X] [Normal2x/3x](https://www.dosbox.com/wiki/Scaler) - DOSBox simple point scaling | [dosbox](https://www.dosbox.com/wiki/Scaler)
- [X] [Kreed's SuperEagle](https://zsnes.com/) - ZSNES optimized variant | [zsnes](https://zsnes.com/)
- [X] [Simple2x/3x/4x](https://wiki.scummvm.org/index.php/Scalers) - ScummVM basic scalers | [scummvm](https://wiki.scummvm.org/index.php/Scalers)
- [X] [TriplePoint](https://wiki.scummvm.org/index.php/Scalers) - ScummVM 3-point interpolation | [scummvm](https://wiki.scummvm.org/index.php/Scalers)
- [X] [FastRotSprite](https://github.com/olegmekekechko) - Oleg Mekekechko (optimized RotSprite for real-time use) | [github](https://github.com/olegmekekechko)
- [X] [Omniscale](https://github.com/libretro/slang-shaders/tree/master/omniscale) - Multi-algorithm combination shader | [impl](https://github.com/nobuyukinyuu/godot-omniscale/blob/master/OmniScale.shader)
- [X] [Sharp-Bilinear](https://github.com/libretro/slang-shaders/tree/master/interpolation) - Integer prescale + bilinear | [impl](https://github.com/libretro/slang-shaders/tree/master/interpolation)
- [X] [Pixellate](https://github.com/libretro/common-shaders/blob/master/retro/shaders/pixellate.cg) - Non-integer nearest-neighbor fix | [impl](https://github.com/libretro/common-shaders/blob/master/retro/shaders/pixellate.cg)
- [X] [Jinc](https://en.wikipedia.org/wiki/Jinc_function) - Bessel-based Lanczos variant | [wiki](https://en.wikipedia.org/wiki/Jinc_function), [libretro](https://github.com/libretro/slang-shaders/tree/master/windowed/shaders/jinc2)
- [X] [Box Filtering](https://en.wikipedia.org/wiki/Box_blur) - Simple area average | [wiki](https://en.wikipedia.org/wiki/Box_blur)

### Handheld/LCD Simulation

- [X] [LCD Grid](https://github.com/libretro/slang-shaders/tree/master/handheld) - cgwg's Game Boy Advance LCD subpixel simulation | [impl](https://github.com/libretro/slang-shaders/tree/master/handheld)
- [X] [Dot Matrix (DMG)](https://github.com/libretro/slang-shaders/tree/master/handheld) - Game Boy DMG dot matrix with green tinting | [impl](https://github.com/libretro/slang-shaders/tree/master/handheld)
- [X] [GBA Color](https://github.com/libretro/slang-shaders/tree/master/handheld/shaders/color) - GBA color profile with gamma and matrix | [impl](https://github.com/libretro/slang-shaders/tree/master/handheld/shaders/color)
- [X] [Game Boy Shader (GBC)](https://github.com/Harlequin-Software/gb-shader) - Game Boy Color LCD simulation | [impl](https://github.com/Harlequin-Software/gb-shader)
- [X] [DS/3DS Color](https://github.com/libretro/slang-shaders/tree/master/handheld) - Nintendo DS color profile with contrast boost | [impl](https://github.com/libretro/slang-shaders/tree/master/handheld)
- [X] [PSP Color](https://github.com/libretro/slang-shaders/tree/master/handheld) - PlayStation Portable vibrant color profile | [impl](https://github.com/libretro/slang-shaders/tree/master/handheld)
- [X] [LCD Ghosting](https://github.com/libretro/slang-shaders/tree/master/motionblur) - Response time blur simulation | [impl](https://github.com/libretro/slang-shaders/tree/master/motionblur)

### Not Yet Implemented - AI/Neural

- [ ] [NNEDI3](https://github.com/sekrit-twc/znedi3) - Neural network edge-directed interpolation | [impl](https://github.com/sekrit-twc/znedi3)
- [ ] [FSRCNNX](https://github.com/igv/FSRCNN-TensorFlow) - Fast Super-Resolution CNN ([paper](https://arxiv.org/abs/1608.00367)) | [impl](https://github.com/igv/FSRCNN-TensorFlow)
- [ ] [ESPCN](https://arxiv.org/abs/1609.05158) - Shi et al. 2016 (sub-pixel convolution) | [paper](https://arxiv.org/abs/1609.05158), [impl](https://github.com/leftthomas/ESPCN)
- [ ] [EDSR](https://arxiv.org/abs/1707.02921) - Lim et al. 2017 (enhanced deep residual) | [paper](https://arxiv.org/abs/1707.02921), [impl](https://github.com/sanghyun-son/EDSR-PyTorch)
- [ ] [RCAN](https://arxiv.org/abs/1807.02758) - Zhang et al. 2018 (residual channel attention) | [paper](https://arxiv.org/abs/1807.02758), [impl](https://github.com/yulunzhang/RCAN)
- [ ] [SAN](https://arxiv.org/abs/1903.10082) - Dai et al. 2019 (second-order attention) | [paper](https://arxiv.org/abs/1903.10082), [impl](https://github.com/daitao/SAN)
- [ ] [Real-ESRGAN](https://github.com/xinntao/Real-ESRGAN) - Wang et al. 2021 ([paper](https://arxiv.org/abs/2107.10833)) | [impl](https://github.com/xinntao/Real-ESRGAN)
- [ ] [SwinIR](https://github.com/JingyunLiang/SwinIR) - Liang et al. 2021 ([paper](https://arxiv.org/abs/2108.10257)) | [impl](https://github.com/JingyunLiang/SwinIR)
- [ ] [ESRGAN](https://arxiv.org/abs/1809.00219) - Wang et al. 2018 (enhanced SRGAN) | [paper](https://arxiv.org/abs/1809.00219), [impl](https://github.com/xinntao/ESRGAN)
- [ ] [SRMD / SRMD-NCNN]() - Zhang et al. 2018 (multiple degradations) | [impl](https://github.com/cszn/SRMD)
- [ ] [LAPAR](https://arxiv.org/abs/2111.12710) - Zhang et al. 2021 (lightweight) | [paper](https://arxiv.org/abs/2111.12710)
- [ ] [Meta-SR](https://arxiv.org/abs/1803.06717) - Hu et al. 2018 (arbitrary scale factors) | [paper](https://arxiv.org/abs/1803.06717), [impl](https://github.com/XuecaiHu/Meta-SR-Pytorch)
- [ ] [TecoGAN](https://arxiv.org/abs/1906.05739) - Chu et al. 2019 (video SR) | [paper](https://arxiv.org/abs/1906.05739), [impl](https://github.com/thunil/TecoGAN)
- [ ] [ACNet](https://arxiv.org/abs/2005.12597) - Gu et al. 2020 (attention-based) | [paper](https://arxiv.org/abs/2005.12597)
- [ ] [HAT-Anime]() | [paper](https://arxiv.org/abs/2309.05239), [impl](https://github.com/XPixelGroup/HAT)
- [ ] [CUGAN]() | [impl](https://github.com/bilibili/ailab/tree/main/Real-CUGAN)
- [ ] [Waifu2x](https://github.com/nagadomi/waifu2x) - nagadomi (anime upscaling) | [impl](https://github.com/nagadomi/waifu2x), [demo](https://waifu2x.udp.jp/)
- [ ] [Anime4K](https://github.com/bloc97/Anime4K) - bloc97 (real-time anime upscaling) | [impl](https://github.com/bloc97/Anime4K)

### Not Yet Implemented - Photographic

- [ ] [Trilinear](https://en.wikipedia.org/wiki/Trilinear_interpolation) / [Tricubic](https://en.wikipedia.org/wiki/Tricubic_interpolation) / [Hermite](https://en.wikipedia.org/wiki/Hermite_interpolation) / [Gaussian](https://en.wikipedia.org/wiki/Gaussian_blur) | [wiki](https://en.wikipedia.org/wiki/Trilinear_interpolation)
- [ ] [Blackman](https://en.wikipedia.org/wiki/Window_function#Blackman_window) / [Hann](https://en.wikipedia.org/wiki/Hann_function) / [Hamming](https://en.wikipedia.org/wiki/Window_function#Hann_and_Hamming_windows) / [Cosine](https://en.wikipedia.org/wiki/Window_function#Cosine_window) - Window function interpolation | [wiki](https://en.wikipedia.org/wiki/Window_function)
- [ ] [Cubic Convolution](https://ieeexplore.ieee.org/document/1163711) - Keys 1981 | [paper](https://ieeexplore.ieee.org/document/1163711)
- [ ] [BEDI](https://ieeexplore.ieee.org/document/4550848) - Bilinear Edge Directed Interpolation | [paper](https://ieeexplore.ieee.org/document/4550848)
- [ ] [EDIUpsizer / eedi3 / EEDI2](https://github.com/HomeOfVapourSynthEvolution/VapourSynth-EEDI2) - Edge-directed family | [impl](https://github.com/HomeOfVapourSynthEvolution/VapourSynth-EEDI2)
- [ ] [SuperRes / NNEDI / NNEDI2](http://avisynth.nl/index.php/Nnedi) - AviSynth neural filters | [wiki](http://avisynth.nl/index.php/Nnedi)
- [ ] [Joint Bilateral Upscaling](https://johanneskopf.de/publications/jbu/) - Kopf et al. 2007 | [project](https://johanneskopf.de/publications/jbu/)
- [ ] [Edge-Guided Image Interpolation](https://ieeexplore.ieee.org/document/5995726) - Zhang & Wu 2008 | [paper](https://ieeexplore.ieee.org/document/5995726)
- [ ] [Image Upscaling via Super-Resolution](https://arxiv.org/abs/1501.00092) - Dong et al. 2014 (SRCNN) | [paper](https://arxiv.org/abs/1501.00092), [impl](https://github.com/yjn870/SRCNN-pytorch)
- [ ] [RAISR](https://ai.googleblog.com/2016/11/enhance-raisr-sharp-images-with-machine.html) - Romano et al. 2016 (Google) | [blog](https://ai.googleblog.com/2016/11/enhance-raisr-sharp-images-with-machine.html), [impl](https://github.com/movehand/raisr)
- [ ] [EDSR / RDN](https://arxiv.org/abs/1707.02921) – pre-GAN high-PSNR era | [paper](https://arxiv.org/abs/1707.02921), [impl](https://github.com/sanghyun-son/EDSR-PyTorch)
- [ ] [RCAN](https://arxiv.org/abs/1807.02758) – attention-based CNN SR | [paper](https://arxiv.org/abs/1807.02758), [impl](https://github.com/yulunzhang/RCAN)
- [ ] [SwinIR](https://arxiv.org/abs/2108.10257) – transformer-based SR | [paper](https://arxiv.org/abs/2108.10257), [impl](https://github.com/JingyunLiang/SwinIR)
- [ ] [HAT (Hybrid Attention Transformer)]() – state of the art (research) | [impl](https://github.com/XPixelGroup/HAT)

## Image Downscaling

- [X] [Adaptive PixelArt Downscaling](https://hiivelabs.com/blog/gamedev/graphics/2025/01/19/adaptive-downscaling-pixel-art/) - HiiveLabs 2025 (edge-aware reduction) | [article](https://hiivelabs.com/blog/gamedev/graphics/2025/01/19/adaptive-downscaling-pixel-art/)
- [X] [Box/Area Averaging](https://en.wikipedia.org/wiki/Image_scaling#Box_sampling) - Simple integer-ratio downscaling | [wiki](https://en.wikipedia.org/wiki/Image_scaling#Box_sampling)
- [X] [Lanczos Downscale](https://en.wikipedia.org/wiki/Lanczos_resampling) - High-quality sinc-based reduction | [wiki](https://en.wikipedia.org/wiki/Lanczos_resampling), [impl](http://www.imagemagick.org/Usage/filter/)
- [X] [Mitchell-Netravali Downscale](https://dl.acm.org/doi/10.1145/54852.378514) - Balanced BC-spline (B=1/3, C=1/3) | [paper](https://dl.acm.org/doi/10.1145/54852.378514)
- [X] [DPID](https://github.com/nickkjolsing/DPID) - Detail-Preserving Perceptual Downscaling ([paper](https://www.sciencedirect.com/science/article/abs/pii/S1077314216301540)) | [impl](https://github.com/nickkjolsing/DPID)
- [X] [Gaussian Downscale](https://en.wikipedia.org/wiki/Gaussian_blur) - Pre-blur with Gaussian kernel | [wiki](https://en.wikipedia.org/wiki/Gaussian_blur)

## Image Rotation

- [X] [RotSprite](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#RotSprite) (Xenowhirl 2007) | [wiki](https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#RotSprite)
- [X] [FastRotSprite](https://github.com/olegmekekechko) (Oleg Mekekechko) - Optimized 4x variant | [github](https://github.com/olegmekekechko)

## Resampling Kernels

Reference: [Getreuer - Linear Methods for Image Interpolation](http://www.ipol.im/pub/art/2011/g_lmii/)

- [X] Rectangular (Box/Nearest) | [wiki](https://en.wikipedia.org/wiki/Box_blur)
- [X] Bicubic (Mitchell-Netravali, alpha=-0.5) | [wiki](https://en.wikipedia.org/wiki/Bicubic_interpolation), [paper](https://dl.acm.org/doi/10.1145/54852.378514)
- [X] Schaum 2 / 3 | [Getreuer](http://www.ipol.im/pub/art/2011/g_lmii/)
- [X] B-Spline 2 / 3 / 5 / 7 / 9 / 11 | [wiki](https://en.wikipedia.org/wiki/B-spline), [Getreuer](http://www.ipol.im/pub/art/2011/g_lmii/)
- [X] O-MOMS 3 / 5 / 7 | [paper](https://ieeexplore.ieee.org/document/826819), [Getreuer](http://www.ipol.im/pub/art/2011/g_lmii/)

## Windowing Functions

Reference: [Harris 1978 - On the Use of Windows for Harmonic Analysis](https://ieeexplore.ieee.org/document/1455106)

- [X] Triangular / Welch | [wiki](https://en.wikipedia.org/wiki/Window_function#Triangular_window)
- [X] Hann (Hanning) / Hamming | [wiki](https://en.wikipedia.org/wiki/Hann_function), [wiki](https://en.wikipedia.org/wiki/Window_function#Hann_and_Hamming_windows)
- [X] Blackman / Nuttall / Blackman-Nuttall / Blackman-Harris | [wiki](https://en.wikipedia.org/wiki/Window_function#Blackman_window)
- [X] FlatTop / Cosine / Power-of-Cosine | [wiki](https://en.wikipedia.org/wiki/Window_function#Flat_top_window)
- [X] Gaussian / Tukey / Poisson | [wiki](https://en.wikipedia.org/wiki/Window_function#Gaussian_window)
- [X] Bartlett-Hann / Hanning-Poisson | [wiki](https://en.wikipedia.org/wiki/Window_function#Bartlett%E2%80%93Hann_window)
- [X] Bohman / Cauchy / Lanczos | [wiki](https://en.wikipedia.org/wiki/Window_function#Bohman_window)
- [X] Kaiser | [wiki](https://en.wikipedia.org/wiki/Kaiser_window)

### Window Function-Based Scalers

These window functions are implemented as image scalers using windowed sinc interpolation:

- [X] [Kaiser](https://en.wikipedia.org/wiki/Kaiser_window) - Adjustable main lobe/sidelobe trade-off using Bessel I0 function
- [X] [Nuttall](https://en.wikipedia.org/wiki/Window_function#Nuttall_window,_continuous_first_derivative) - 4-term cosine sum with excellent sidelobe suppression
- [X] [Bartlett](https://en.wikipedia.org/wiki/Window_function#Triangular_window) - Triangular window, simple and efficient
- [X] [Welch](https://en.wikipedia.org/wiki/Window_function#Welch_window) - Parabolic window with good frequency response
- [X] [Tukey](https://en.wikipedia.org/wiki/Window_function#Tukey_window) - Rectangle with cosine-tapered edges

## Color Interpolation

### Standard Interpolators

- [X] RGB Linear Interpolation (byte and normalized) | [wiki](https://en.wikipedia.org/wiki/Linear_interpolation)
- [X] HSV/HSL/HWB Interpolation with circular hue handling | [article](https://www.alanzucconi.com/2016/01/06/colour-interpolation/)
- [X] Lab/Luv/Lch Interpolation | [Lindbloom](http://www.brucelindbloom.com/)
- [X] Generic `ColorSpaceLerp<TColorSpace>` for any color space

### Special Interpolators

- [X] Circular Hue Lerp - For cylindrical color spaces | [article](https://www.alanzucconi.com/2016/01/06/colour-interpolation/)
- [X] Color Gradients - Multi-stop gradient generation | [CSS](https://developer.mozilla.org/en-US/docs/Web/CSS/gradient/linear-gradient)

## Vision Simulation

Simulate how colors appear to observers with different visual systems.

### Human Color Vision Deficiencies

References:
- [Brettel et al. 1997](https://vision.psychol.cam.ac.uk/jdmollon/papers/Brettel1997.pdf) - Computerized simulation of color appearance for dichromats
- [Viénot et al. 1999](https://onlinelibrary.wiley.com/doi/abs/10.1002/(SICI)1520-6378(199908)24:4%3C243::AID-COL5%3E3.0.CO;2-6) - Digital video colourmaps for colorblind viewers
- [Machado et al. 2009](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html) - Physiologically-based model

- [ ] [Protanopia](https://en.wikipedia.org/wiki/Color_blindness#Protanopia) - Red-blind (missing L-cones, ~1% males) | [impl](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html), [coblis](https://www.color-blindness.com/coblis-color-blindness-simulator/)
- [ ] [Protanomaly](https://en.wikipedia.org/wiki/Color_blindness#Protanomaly) - Red-weak (shifted L-cones, ~1% males) | [impl](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html)
- [ ] [Deuteranopia](https://en.wikipedia.org/wiki/Color_blindness#Deuteranopia) - Green-blind (missing M-cones, ~1% males) | [impl](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html), [coblis](https://www.color-blindness.com/coblis-color-blindness-simulator/)
- [ ] [Deuteranomaly](https://en.wikipedia.org/wiki/Color_blindness#Deuteranomaly) - Green-weak (shifted M-cones, ~5% males) | [impl](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html)
- [ ] [Tritanopia](https://en.wikipedia.org/wiki/Color_blindness#Tritanopia) - Blue-blind (missing S-cones, rare) | [impl](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html)
- [ ] [Tritanomaly](https://en.wikipedia.org/wiki/Color_blindness#Tritanomaly) - Blue-weak (shifted S-cones, rare) | [impl](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html)
- [ ] [Achromatopsia](https://en.wikipedia.org/wiki/Achromatopsia) - Complete color blindness (rod monochromacy) | [wiki](https://en.wikipedia.org/wiki/Achromatopsia)
- [ ] [Blue Cone Monochromacy](https://en.wikipedia.org/wiki/Blue_cone_monochromacy) - Only S-cones functional | [wiki](https://en.wikipedia.org/wiki/Blue_cone_monochromacy)

### Animal Vision Simulation

Simulate how various species perceive colors based on their photoreceptor configurations.

References:
- [Kelber et al. 2003](https://onlinelibrary.wiley.com/doi/10.1046/j.1095-8312.2003.00217.x) - Animal colour vision – behavioural tests and physiological concepts
- [Osorio & Vorobyev 2008](https://www.sciencedirect.com/science/article/pii/S0042698908000564) - A review of the evolution of animal colour vision
- [Marshall & Oberwinkler 1999](https://www.nature.com/articles/43327) - Mantis shrimp colour vision

#### Dichromatic Vision (2 cone types)

- [ ] [Canine (Dogs)](https://www.sciencedirect.com/science/article/abs/pii/0042698989900841) - Miller & Murphy 1995 (~430nm, ~555nm blue-yellow) | [article](https://www.psychologytoday.com/us/blog/canine-corner/201010/can-dogs-see-colors), [paper](https://www.sciencedirect.com/science/article/abs/pii/0042698989900841)
- [ ] [Feline (Cats)](https://pubmed.ncbi.nlm.nih.gov/8058261/) - Loop et al. 1987 (similar to canine) | [paper](https://pubmed.ncbi.nlm.nih.gov/8058261/)
- [ ] [Most Mammals](https://en.wikipedia.org/wiki/Color_vision#Mammals) - Ancestral mammalian dichromacy | [wiki](https://en.wikipedia.org/wiki/Color_vision#Mammals)

#### Trichromatic Vision (3 cone types)

- [ ] [Old World Primates](https://www.annualreviews.org/doi/abs/10.1146/annurev.neuro.27.070203.144220) - Jacobs 2008 (human-like) | [paper](https://www.annualreviews.org/doi/abs/10.1146/annurev.neuro.27.070203.144220)
- [ ] [Honeybee (Apis)](https://jeb.biologists.org/content/207/14/2507) - Peitsch et al. 1992 (~340nm UV, ~430nm, ~535nm) | [paper](https://jeb.biologists.org/content/207/14/2507)
- [ ] [Goldfish (Carassius)](https://pubmed.ncbi.nlm.nih.gov/16325267/) - Neumeyer 1992 (tetrachromat with UV) | [paper](https://pubmed.ncbi.nlm.nih.gov/16325267/)

#### Tetrachromatic Vision (4 cone types)

- [ ] [Birds (Aves)](https://pubmed.ncbi.nlm.nih.gov/11283361/) - Hart 2001 (UV/Violet + Blue + Green + Red) | [paper](https://pubmed.ncbi.nlm.nih.gov/11283361/)
- [ ] [Zebrafish (Danio)](https://pubmed.ncbi.nlm.nih.gov/20566361/) - Allison et al. 2010 (UV + Blue + Green + Red) | [paper](https://pubmed.ncbi.nlm.nih.gov/20566361/)
- [ ] [Swallowtail Butterfly (Papilio)](https://pubmed.ncbi.nlm.nih.gov/10327129/) - Arikawa et al. 1999 (up to 6 receptor types) | [paper](https://pubmed.ncbi.nlm.nih.gov/10327129/)
- [ ] [Reptiles](https://pubmed.ncbi.nlm.nih.gov/17166524/) - Bowmaker 2008 (UV-sensitive tetrachromat) | [paper](https://pubmed.ncbi.nlm.nih.gov/17166524/)

#### Pentachromatic and Beyond (5+ receptor types)

- [ ] [Cephalopods (Octopus, Cuttlefish)](https://www.pnas.org/doi/10.1073/pnas.1524578113) - Stubbs & Stubbs 2016 (chromatic aberration sensing) | [paper](https://www.pnas.org/doi/10.1073/pnas.1524578113)
- [ ] [Mantis Shrimp (Stomatopoda)](https://www.science.org/doi/10.1126/science.1245824) - Thoen et al. 2014 (12-16 receptors, 300-720nm) | [paper](https://www.science.org/doi/10.1126/science.1245824)
- [ ] [Dragonfish (Malacosteus)](https://pubmed.ncbi.nlm.nih.gov/10101115/) - Douglas et al. 1999 (~700nm far-red bioluminescence) | [paper](https://pubmed.ncbi.nlm.nih.gov/10101115/)
- [ ] [Papilio xuthus](https://pubmed.ncbi.nlm.nih.gov/10327129/) - Arikawa et al. 1999 (6 photoreceptor classes) | [paper](https://pubmed.ncbi.nlm.nih.gov/10327129/)

#### Specialized Vision

- [ ] [Insect Compound Eye](https://en.wikipedia.org/wiki/Compound_eye) - Flicker fusion, motion detection | [wiki](https://en.wikipedia.org/wiki/Compound_eye)
- [ ] [UV Vision Visualization](https://www.bbc.com/news/science-environment-22293946) - Generic UV channel rendering | [article](https://www.bbc.com/news/science-environment-22293946)
- [ ] [Polarization Vision](https://royalsocietypublishing.org/doi/10.1098/rstb.2010.0203) - Horváth et al. 2011 | [paper](https://royalsocietypublishing.org/doi/10.1098/rstb.2010.0203)

## Chromatic Adaptation

Transform colors between different illuminant conditions.

References:
- [Lindbloom - Chromatic Adaptation](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html) - Comprehensive reference
- [Fairchild 2013](https://www.wiley.com/en-us/Color+Appearance+Models,+3rd+Edition-p-9781119967033) - Color Appearance Models (3rd ed.)
- [CIE 015:2018](https://cie.co.at/publications/colorimetry-4th-edition) - Colorimetry (4th ed.)

### Adaptation Transforms

- [ ] [Bradford Transform](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html) - Lam 1985 (sharpened cone response, most accurate) | [impl](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html), [colour](https://colour.readthedocs.io/)
- [ ] [Von Kries Transform](https://en.wikipedia.org/wiki/Von_Kries_coefficient_law) - Von Kries 1902 (diagonal scaling) | [wiki](https://en.wikipedia.org/wiki/Von_Kries_coefficient_law), [Lindbloom](http://www.brucelindbloom.com/)
- [ ] [XYZ Scaling](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html) - Simple XYZ ratio scaling | [impl](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html)
- [ ] [CAT02](https://en.wikipedia.org/wiki/CIECAM02#CAT02) - CIE 2002 (CIECAM02 standard) | [wiki](https://en.wikipedia.org/wiki/CIECAM02#CAT02), [colour](https://colour.readthedocs.io/)
- [ ] [CAT16](https://observablehq.com/@jrus/cam16) - Li et al. 2017 (improved for CIECAM16) | [impl](https://observablehq.com/@jrus/cam16), [colour](https://colour.readthedocs.io/)
- [ ] [CIE 1994](https://cie.co.at/publications/review-chromatic-adaptation-transforms) - CIE 1994 Technical Report | [CIE](https://cie.co.at/), [colour](https://colour.readthedocs.io/en/latest/generated/colour.chromatic_adaptation.html)
- [ ] [CMCCAT97](https://doi.org/10.1002/col.5080220605) - Li et al. 1997 (CMC standard) | [paper](https://doi.org/10.1002/col.5080220605), [colour](https://colour.readthedocs.io/)
- [ ] [CMCCAT2000](https://doi.org/10.1002/col.10020) - Li et al. 2002 (improved CMC) | [paper](https://doi.org/10.1002/col.10020), [colour](https://colour.readthedocs.io/en/latest/generated/colour.adaptation.chromatic_adaptation_CMCCAT2000.html)
- [ ] [Fairchild 1990](https://doi.org/10.1002/col.5080150405) - Fairchild 1990 (incomplete adaptation) | [paper](https://doi.org/10.1002/col.5080150405), [colour](https://colour.readthedocs.io/)
- [ ] [Sharp](https://doi.org/10.1117/12.410788) - Süsstrunk et al. 2000 (sharp cone fundamentals) | [paper](https://doi.org/10.1117/12.410788), [colour](https://colour.readthedocs.io/)
- [ ] [Zhai 2018](https://doi.org/10.1364/OE.26.007724) - Zhai & Luo 2018 (two-step adaptation) | [paper](https://doi.org/10.1364/OE.26.007724), [colour](https://colour.readthedocs.io/en/latest/generated/colour.adaptation.chromatic_adaptation_Zhai2018.html)

### Standard Illuminants

Reference: [CIE Standard Illuminants](https://cie.co.at/publications/cie-015-2018-colorimetry-4th-edition)

- [ ] [D50](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html) - Horizon daylight (5003K, print/photography standard) | [Lindbloom](http://www.brucelindbloom.com/)
- [ ] [D65](http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html) - Noon daylight (6504K, display/video standard) | [Lindbloom](http://www.brucelindbloom.com/)
- [ ] [D55](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D) - Mid-morning daylight (5503K) | [wiki](https://en.wikipedia.org/wiki/Standard_illuminant)
- [ ] [D75](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D) - North sky daylight (7504K) | [wiki](https://en.wikipedia.org/wiki/Standard_illuminant)
- [ ] [Illuminant A](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_A) - Incandescent tungsten (2856K) | [wiki](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_A)
- [ ] [Illuminant F2](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_F) - Cool white fluorescent (4230K) | [wiki](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_F)
- [ ] [Illuminant F11](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_F) - Narrow-band fluorescent (4000K) | [wiki](https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_F)
- [ ] [LED-B3](https://cie.co.at/publications/cie-015-2018-colorimetry-4th-edition) - Modern LED illuminant (CIE 2018) | [CIE](https://cie.co.at/publications/cie-015-2018-colorimetry-4th-edition)

## Tone Mapping

Convert between dynamic ranges (HDR ↔ SDR).

References:
- [Reinhard et al. 2010](https://www.cs.utah.edu/~reinhard/cdrom/) - High Dynamic Range Imaging (2nd ed.)
- [Banterle et al. 2017](https://www.wiley.com/en-us/Advanced+High+Dynamic+Range+Imaging,+2nd+Edition-p-9781498706940) - Advanced HDR Imaging

### Global Operators

- [ ] [Reinhard Global](https://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf) - Reinhard et al. 2002 (SIGGRAPH) | [paper](https://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf), [impl](https://github.com/banterle/HDR_Toolbox)
- [ ] [Reinhard Extended](https://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf) - With white point parameter | [paper](https://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf)
- [ ] [Exponential](https://en.wikipedia.org/wiki/Tone_mapping#Methods) - Exponential compression | [wiki](https://en.wikipedia.org/wiki/Tone_mapping)
- [ ] [Logarithmic](https://en.wikipedia.org/wiki/Tone_mapping#Methods) - Log-based compression | [wiki](https://en.wikipedia.org/wiki/Tone_mapping)
- [ ] [Drago Logarithmic](https://resources.mpi-inf.mpg.de/tmo/logmap/) - Drago et al. 2003 (adaptive log) | [project](https://resources.mpi-inf.mpg.de/tmo/logmap/)

### Filmic Operators

- [ ] [Filmic (Hable/Uncharted 2)](http://filmicworlds.com/blog/filmic-tonemapping-operators/) - John Hable 2010 (GDC) | [blog](http://filmicworlds.com/blog/filmic-tonemapping-operators/), [shadertoy](https://www.shadertoy.com/view/lslGzl)
- [ ] [ACES Filmic](https://github.com/ampas/aces-dev) - Academy 2014 ([RRT+ODT](https://docs.acescentral.com/)) | [impl](https://github.com/ampas/aces-dev), [docs](https://docs.acescentral.com/)
- [ ] [AgX](https://github.com/sobotka/AgX) - Troy Sobotka 2023 (Blender 4.0+) | [impl](https://github.com/sobotka/AgX)
- [ ] [Khronos PBR Neutral](https://github.com/KhronosGroup/ToneMapping) - Khronos 2023 (glTF standard) | [impl](https://github.com/KhronosGroup/ToneMapping)

### Local Operators

- [ ] [Reinhard Local](https://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf) - Reinhard et al. 2002 (dodging & burning) | [paper](https://www.cs.utah.edu/~reinhard/cdrom/tonemap.pdf)
- [ ] [Durand Bilateral](https://people.csail.mit.edu/fredo/PUBLI/Siggraph2002/) - Durand & Dorsey 2002 (SIGGRAPH) | [project](https://people.csail.mit.edu/fredo/PUBLI/Siggraph2002/)
- [ ] [Fattal Gradient Domain](https://www.cs.huji.ac.il/~dan101/hdrc/) - Fattal et al. 2002 (gradient attenuation) | [project](https://www.cs.huji.ac.il/~dan101/hdrc/)
- [ ] [Mantiuk](http://resources.mpi-inf.mpg.de/hdr/ldr2hdr/) - Mantiuk et al. 2008 (perceptual contrast) | [project](http://resources.mpi-inf.mpg.de/hdr/ldr2hdr/)

## Gamut Mapping

Handle out-of-gamut colors when converting between color spaces.

References:
- [Morovic 2008](https://www.wiley.com/en-us/Color+Gamut+Mapping-p-9780470030325) - Color Gamut Mapping (Wiley)
- [CIE 156:2004](https://cie.co.at/publications/guidelines-evaluation-gamut-mapping-algorithms) - Guidelines for Gamut Mapping Evaluation

### Clipping Methods

- [ ] [RGB Clipping](https://www.colour-science.org/posts/gamut-mapping-and-clipping/) - Simple per-channel clamp (may shift hue) | [article](https://www.colour-science.org/posts/gamut-mapping-and-clipping/), [colour](https://colour.readthedocs.io/)
- [ ] [Chroma Clipping](https://www.colour-science.org/posts/gamut-mapping-and-clipping/) - Reduce chroma preserving hue/lightness | [article](https://www.colour-science.org/posts/gamut-mapping-and-clipping/)
- [ ] [Lightness Clipping](https://www.colour-science.org/posts/gamut-mapping-and-clipping/) - Adjust lightness to fit gamut | [article](https://www.colour-science.org/posts/gamut-mapping-and-clipping/)

### Compression Methods

- [ ] Linear Compression - Scale entire gamut proportionally | [colour](https://colour.readthedocs.io/)
- [ ] [Chroma Compression](https://www.colour-science.org/posts/gamut-mapping-and-clipping/) - Compress chroma toward neutral axis | [article](https://www.colour-science.org/posts/gamut-mapping-and-clipping/)
- [ ] [Cusp Mapping](https://www.colour-science.org/posts/gamut-mapping-and-clipping/) - Map toward gamut cusp | [article](https://www.colour-science.org/posts/gamut-mapping-and-clipping/)
- [ ] [MINDE (Minimum ΔE)](https://www.colour-science.org/posts/gamut-mapping-and-clipping/) - Perceptually optimal mapping | [article](https://www.colour-science.org/posts/gamut-mapping-and-clipping/)

### Standards

- [ ] [ICC.1 Perceptual Intent](https://www.color.org/specification/ICC.1-2022-05.pdf) - ICC profile perceptual rendering | [spec](https://www.color.org/specification/ICC.1-2022-05.pdf)
- [ ] [ICC.1 Saturation Intent](https://www.color.org/specification/ICC.1-2022-05.pdf) - ICC profile saturation rendering | [spec](https://www.color.org/specification/ICC.1-2022-05.pdf)
- [ ] [CSS Color Level 4 Gamut Mapping](https://www.w3.org/TR/css-color-4/#gamut-mapping) - Web standard algorithm | [spec](https://www.w3.org/TR/css-color-4/#gamut-mapping), [CSS](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value)

## Palette Operations

Tools for working with color palettes.

Reference: [Colour Sorting](https://www.alanzucconi.com/2015/09/30/colour-sorting/) - Alan Zucconi

### Palette Extraction

- [ ] [Dominant Color Extraction](https://en.wikipedia.org/wiki/Color_quantization) - Find N most representative colors | [impl](https://github.com/lokesh/color-thief), [vibrant.js](https://jariz.github.io/vibrant.js/)
- [ ] [Theme Extraction](https://material.io/blog/science-of-color-design) - Google Material Design color system | [article](https://material.io/blog/science-of-color-design)
- [ ] [Swatch Sampling](https://en.wikipedia.org/wiki/Color_picker) - Uniform/stratified sampling from image | [wiki](https://en.wikipedia.org/wiki/Color_picker)

### Palette Sorting

- [ ] [Hilbert Curve Sort](https://en.wikipedia.org/wiki/Hilbert_curve) - Space-filling curve order | [wiki](https://en.wikipedia.org/wiki/Hilbert_curve), [impl](https://github.com/davemc0/hilbertsorting)
- [ ] [HSL/HSV Sort](https://www.alanzucconi.com/2015/09/30/colour-sorting/) - Hue-primary sorting | [article](https://www.alanzucconi.com/2015/09/30/colour-sorting/)
- [ ] [Luminance Sort](https://en.wikipedia.org/wiki/Luma_(video)) - Light-to-dark ordering | [wiki](https://en.wikipedia.org/wiki/Luma_(video))
- [ ] [Step Sort](https://www.alanzucconi.com/2015/09/30/colour-sorting/) - Multi-pass hue/luma sort | [article](https://www.alanzucconi.com/2015/09/30/colour-sorting/)
- [ ] [Traveling Salesman Sort](https://en.wikipedia.org/wiki/Travelling_salesman_problem) - Minimize total color distance | [wiki](https://en.wikipedia.org/wiki/Travelling_salesman_problem), [impl](https://github.com/dmishin/tsp-solver)

### Palette Comparison

- [ ] [Palette Similarity (ΔE)](https://zschuessler.github.io/DeltaE/learn/) - Mean/Max color difference between palettes | [tutorial](https://zschuessler.github.io/DeltaE/learn/), [impl](https://github.com/zschuessler/DeltaE)
- [ ] [Coverage Analysis](https://en.wikipedia.org/wiki/Gamut) - How well palette covers color space | [wiki](https://en.wikipedia.org/wiki/Gamut)
- [ ] [Color Histogram Intersection](https://ieeexplore.ieee.org/document/323794) - Swain & Ballard 1991 | [paper](https://ieeexplore.ieee.org/document/323794)

### Palette Generation

Reference: [Color Schemes](https://en.wikipedia.org/wiki/Color_scheme) - Color theory basics

- [ ] [Complementary Colors](https://en.wikipedia.org/wiki/Complementary_colors) - Opposite hue pairs (180°) | [wiki](https://en.wikipedia.org/wiki/Complementary_colors), [paletton](https://paletton.com/)
- [ ] [Triadic Colors](https://en.wikipedia.org/wiki/Color_scheme#Triadic) - Three evenly spaced hues (120°) | [wiki](https://en.wikipedia.org/wiki/Color_scheme#Triadic)
- [ ] [Analogous Colors](https://en.wikipedia.org/wiki/Color_scheme#Analogous) - Adjacent hues (30° apart) | [wiki](https://en.wikipedia.org/wiki/Color_scheme#Analogous)
- [ ] [Split-Complementary](https://en.wikipedia.org/wiki/Color_scheme#Split-complementary) - Complement + two adjacent (150°, 210°) | [wiki](https://en.wikipedia.org/wiki/Color_scheme#Split-complementary)
- [ ] [Tetradic/Square Colors](https://en.wikipedia.org/wiki/Color_scheme#Tetradic) - Four evenly spaced hues (90°) | [wiki](https://en.wikipedia.org/wiki/Color_scheme#Tetradic)

---

## Architecture & Design Goals

> Zero-Cost Color Processing & Scaling Framework

STATUS: **DESIGN FROZEN**

## 0. Core goals (non-negotiable)

1. **Zero-cost abstractions**

   * No virtual dispatch in hot paths
   * No boxing
   * Struct-only strategies
   * Generic dispatch so JIT can inline and specialize
2. **Correct color math**

   * No interpolation in gamma-encoded byte space
   * No forced perceptual space for math
3. **Performance tunability**

   * Same scaler must run “RGBA-byte fast” or “perceptual HQ” via generics
4. **Composable pipeline**

   * Avoid unnecessary encode/decode
   * Stay in working space as long as possible
5. **Bitmap-centric**

   * `System.Drawing.Bitmap` / `Color` are I/O only
   * Never used in hot loops

---

## 1. The three spaces (this is the backbone)

There are **exactly three roles**. Never collapse them.

### 1.1 Storage space (`TPixel`)

* What is physically stored in memory / bitmaps
* Byte-oriented, tightly packed
* No math, no distance, no interpolation
* Examples:

  * `Rgb24`, `Rgb15`, `Rgb16`
  * `Rgba32`
  * `Rgb48`, `Rgba64`

**Framework dependency rule**

Space modules and core strategies must not reference `System.Drawing` (or other UI/image frameworks).  
All conversions to/from framework types (`Color`, `Bitmap`) must be implemented in the adapter layer (e.g. `Imaging.SystemDrawing`) using storage formats (`Rgba32`, etc.) as the interchange.


### 1.2 Working space (`TWork`)

* Where **math happens**
* Interpolation, accumulation, error diffusion
* Open ranges allowed (negative, >1)
* Straight alpha
* Examples:

  * `LinearRgbF`
  * `LinearRgbaF`

### 1.3 Key space (`TKey`)

* Where **decisions happen**
* Equality and distance only
* Cheap or perceptual
* Examples:

  * `Rgba32` (fast mode)
  * `YuvF`, `YCoCg`
  * `LabF`
  * packed `uint` keys

### Invariant

* **Math → working space**
* **Decisions → key space**
* **Bytes → storage space**

---

## 2. Color space tagging & shape contracts

### 2.1 Marker

```csharp
interface IColorSpace { }
```

### 2.2 Shape (alpha handled separately)

```csharp
interface IColorSpace3B<T> : IColorSpace
  where T : unmanaged, IColorSpace3B<T> {
  static abstract byte C0(in T c);
  static abstract byte C1(in T c);
  static abstract byte C2(in T c);
}

interface IColorSpace3F<T> : IColorSpace
  where T : unmanaged, IColorSpace3F<T> {
  static abstract float C0(in T c);
  static abstract float C1(in T c);
  static abstract float C2(in T c);
}
```

Rules:

* No range guarantees
* No gamma semantics
* No alpha

### 2.3 Space modules and discoverability

Color space structs (`YuvF`, `LabF`, `LinearRgbF`, etc.) are **dumb data**: immutable, unmanaged, and free of framework dependencies.

All space-specific logic (conversions, projectors, visualization helpers, presets) must be co-located next to the space type in a dedicated space module folder/namespace (e.g. `Spaces/Yuv/*`).

To keep discoverability high without bloating the struct, each space module must provide a single static entry point class (a "companion") that exposes the recommended presets and helpers for that space.

**Recommended module layout (discoverability)**

```
ColorProcessing/
  Storage/
    Rgb24.cs
    Rgb15.cs
    Rgb16.cs
    Rgba32.cs
    Rgb48.cs
    Rgba64.cs
  Working/
    LinearRgbF.cs
    LinearRgbaF.cs
  Spaces/
    Yuv/
      YuvF.cs
      Yuv.cs                   // companion entry point
      YuvConverters.cs         // Yuv <-> LinearRgbF / Xyz variants
      YuvProjectors.cs         // TWork -> Yuv key extractors
      YuvVisualizers.cs        // debug plane visualization defaults
    Lab/
      LabF.cs
      Lab.cs
      LabConverters.cs
      LabProjectors.cs
      LabVisualizers.cs
  Metrics/
    Manhattan3.cs
    Euclidean3.cs
    EuclideanSquared3.cs
    Chebyshev3.cs
    Weighted*.cs
Lockers/
  IBitmapLocker.cs
  Rgba32BitmapLocker.cs
  Rgb24BitmapLocker.cs
  Rgb15BitmapLocker.cs
  Rgb16BitmapLocker.cs
  Argb16BitmapLocker.cs
Drawing/
  Bitmap.cs                 // Extensions for System.Drawing.Bitmap
  Color.cs                  // Extensions for System.Drawing.Color  
```

Rules:

- Core modules (Storage, Working, Spaces, Metrics) must not reference System.Drawing.
- Imaging.SystemDrawing is the adapter layer and contains all Bitmap/Color glue.
```

### 2.4 Space companion entry point (single entry point)

Each color space module must provide a single static entry point class named after the space (e.g. `Yuv`, `Lab`, `Rgb`, `Xyz`).  
This companion class is the discoverable "grab bag" for:

- conversion presets (matrix variants, white points, ranges)
- projectors (working → key)
- plane/channel visualization defaults
- optional space constants/metadata (plane names, typical ranges)

Rules:

- The companion class may expose `readonly` strategy values/instances (structs) and factory methods.
- The companion class must not require storing strategies behind interfaces.
- Framework adapters (`System.Drawing.Color`, `Bitmap`) must not be referenced in the core space module; they live in the adapter layer (see Section 12/Imaging module).
- The space struct itself must remain minimal: fields + shape contracts only.

**Companion example (illustrative)**

```csharp
public static class Yuv
{
  public static readonly Yuv601FullToLinearRgbF Bt601Full = default;
  public static readonly Yuv709FullToLinearRgbF Bt709Full = default;

  public static readonly ProjectLinearRgbFToYuv601Full Project601Full = default;

  public static readonly YuvPlaneVisualizer Visualizer = default;
}
```

This keeps "everything YUV" discoverable under `Yuv.*` while keeping the `YuvF` struct minimal.

---

## 3. Storage codecs (I/O boundary)

### 3.1 Decode / Encode

```csharp
interface IDecode<TPixel, TWork>
  where TPixel : unmanaged
  where TWork : unmanaged {
  TWork Decode(in TPixel p);
}

interface IEncode<TWork, TPixel>
  where TWork : unmanaged
  where TPixel : unmanaged {
  TPixel Encode(in TWork w);
}
```

Rules:

* Decode: storage → working
* Encode: working → storage
* Gamma, clamping, quantization live here
* Alpha policy defined here (straight vs premultiplied)

`System.Drawing.Bitmap` and `Color` **never** appear beyond this boundary.

---

## 4. Projection (working → key)

```csharp
interface IProject<TWork, TKey>
  where TWork : unmanaged
  where TKey : unmanaged {
  TKey Project(in TWork w);
}
```

Purpose:

* Extract a **decision key**
* May drop channels
* May include or exclude alpha

---

## 5. Decision logic (key space)

### 5.1 Equality

```csharp
interface IColorEquality<TKey>
  where TKey : unmanaged {
  bool Equals(in TKey a, in TKey b);
}
```

* Exact
* Threshold-based
* Runtime configurable (stateful struct allowed)

### 5.2 Distance / metric

```csharp
interface IColorMetric<TKey>
  where TKey : unmanaged {
  float Distance(in TKey a, in TKey b);
}
```

Required families:

* Manhattan
* Euclidean
* EuclideanSquared (no sqrt)
* Chebyshev
* Weighted variants (runtime weights)

Metrics **always operate in key space**.

---

## 6. Math contracts (working space)

### 6.1 Interpolation

```csharp
interface ILerp<TWork>
  where TWork : unmanaged {
  TWork Lerp(in TWork a, in TWork b, float t);
}
```

### 6.2 Accumulation (resampling)

```csharp
interface IAccum<TWork>
  where TWork : unmanaged {
  static abstract TWork Zero();
  static abstract TWork AddMul(in TWork acc, in TWork x, float w);
}
```

### 6.3 Error diffusion

```csharp
interface IErrorOps<TWork>
  where TWork : unmanaged {
  TWork Sub(in TWork a, in TWork b);
  TWork AddScaled(in TWork a, in TWork err, float scale);
}
```

---

## 7. Alpha handling (explicit rules)

* Storage formats may have alpha
* `TWork` always uses **straight alpha**
* Decode converts alpha to float
* Encode clamps alpha
* `IProject` decides whether alpha participates in decisions
* Resamplers interpolate alpha linearly

No implicit premultiplication.

Alpha does not participate in key-space equality or distance unless explicitly included by the `IProject` implementation.

---

## 8. Scaler kernel (generic core)

All scaler kernels must be expressible as:

```csharp
static void Scale<
  TWork,
  TKey,
  TProject,
  TMetric,
  TEquality,
  TLerp,
  TAccum>(
  ReadOnlySpan<TWork> src, int srcW, int srcH,
  Span<TWork> dst, int dstW, int dstH,
  in TProject project,
  in TMetric metric,
  in TEquality equality,
  in TLerp lerp,
  in TAccum accum)
```

Rules:

* Kernel reads/writes **only `TWork`**
* Decisions are made via projected `TKey`
* Unused strategies cost nothing if not referenced

---

## 9. Pipeline that stays in working space

### 9.1 Pipeline invariant

> Once decoded into `TWork`, the pipeline **never returns to storage**
> until explicitly encoded.

### 9.2 Frame types

* `PixelFrame<TPixel>`
  Wraps bitmap lock or owned storage buffer

* `WorkFrame<TWork>`
  Owns pooled working buffers

All pipeline steps operate on `TWork`; the pipeline never directly operates on `TPixel` except at explicit decode/encode boundaries.

### 9.3 Work pipeline

```csharp
ref struct WorkPipeline<TWork>
  where TWork : unmanaged
{
  // Span<TWork> buffer
  // width, height
  // pooled ownership
}
```

### 9.4 Pipeline steps

#### Decode (boundary)

```csharp
Bitmap → WorkPipeline<TWork>
```

#### Work steps (composable)

```csharp
pipeline
  .Scale<Eagle>(...)
  .Scale<Hq2x>(...)
  .Resample<Bilinear>(...)
```

#### Encode (boundary)

```csharp
WorkPipeline<TWork> → Bitmap
```

### 9.5 Buffer reuse

* Use `ArrayPool<TWork>`
* Ping-pong buffers between steps
* Reallocate only when dimensions change

---

### 9.6 Lifetime & ownership

- `WorkPipeline<TWork>` owns its working buffers.
- Working buffers are rented from `ArrayPool<TWork>`.
- Buffers are returned when:
  - `.ToBitmap()` / `.Encode()` is called, or
  - the pipeline is disposed.
- Pipelines are **single-use** and **not thread-safe**.
- Chained operations reuse buffers when dimensions permit; new buffers are rented only when size changes.

---

## 10. Public API surface

### 10.1 Bitmap entry points

#### Generic

```csharp
bitmap.Scale<TScaler>(bool highQuality = false)
bitmap.Resample<TResampler>(Size target, bool highQuality = false)
```

#### Instance-based (runtime config)

```csharp
bitmap.Scale(in scalerInstance, bool highQuality = false)
bitmap.Resample(in resamplerInstance, Size target, bool highQuality = false)
```

### 10.2 Pipeline usage

```csharp
using var result = bitmap
    .ToWork<LinearRgbaF, DecodeSrgb32ToLinearRgbaF>()
    .Scale<Eagle>(highQuality: false)
    .Scale(Hqx.Instance4x, highQuality: true)
    .Resample<Bilinear>(new Size(w, h))
    .ToBitmap<Rgba32, EncodeLinearRgbaFToSrgb32>();
```

No intermediate encode/decode.

---

## 11. Scaler families

### Pixel-art scalers (Scale2x, xBR, Eagle, HQx)

* `TWork`: `LinearRgbaF` (HQ) or identity (fast)
* `TKey`: `Rgba32`, `YuvF`, or `LabF`
* Decisions only, minimal interpolation

Pixel-art scalers always operate in working space (`TWork`) to ensure consistent handling of alpha and any policy-driven equality that may treat distinct values as equivalent.
For maximum performance, `TWork` may be identical to `TPixel` and the selected decode/encode/project strategies may be identity strategies; in that case, JIT inlining should eliminate all conversion overhead.

### Resamplers (bilinear, bicubic, Lanczos)

* `TWork`: `LinearRgbaF`
* Use `IAccum` + `ILerp`
* No decision logic required

---

## 12. Storage formats to implement

Minimum:

* `Rgb24`
* `Rgb15`
* `Rgb16`
* `Rgba32`
* `Rgb48`
* `Rgba64`

Each must have decode/encode to at least one working space.

---

## 13. Presets (ergonomics layer)

End users should never see generics.

Provide:

```csharp
bitmap.Scale<Eagle>(highQuality: false)
bitmap.Scale(Hqx.Instance4x, highQuality: true)
bitmap.Resample<Lanczos>(new Size(1024, 1024))
bitmap.Resample(Kernels.Moms3, new Size(768, 768))
```

Internally these select concrete strategy sets.

---

## 14. Debug & Plane visualization (presentation transforms)

Plane/channel visualization is **not** a color space conversion.  
It is a presentation transform used for debugging/inspection (e.g. show Y as grayscale, show U/V as signed ramps).

Visualization runs at the boundaries (typically producing `Rgba32` or `Bitmap`) and must be explicit and configurable.

A space module may provide a default visualizer (e.g. `Yuv.Visualizer`) that knows the channel meaning and default normalization/range assumptions.

### Plane visualization contract

A plane visualizer maps one color-space sample + plane index to a display pixel.

```csharp
public interface IPlaneVisualizer<TSpace, TPixel>
  where TSpace : unmanaged
  where TPixel : unmanaged
{
  int PlaneCount { get; }
  ReadOnlySpan<char> PlaneName(int planeIndex);

  TPixel Visualize(in TSpace c, int planeIndex);
}

```

**Rules:**

* Visualizers are struct strategies and may carry runtime state (scale/bias/min/max).
* `planeIndex` is 0..PlaneCount-1.
* Alpha is handled explicitly: either visualized as a dedicated plane or ignored based on the visualizer.

---

## 15. Explicit non-goals

* No inheritance trees for colors
* No polymorphic storage of strategies
* No RGBA8 as universal hub
* No hidden gamma correction
* No automatic alpha premultiplication
* No mid-pipeline working-space switching

- No mid-pipeline working-space switching (e.g. LinearRGB → Lab → LinearRGB)

---

## 15. One-sentence mental model

> **Bytes are storage, floats are math, keys are decisions — everything else is policy.**
