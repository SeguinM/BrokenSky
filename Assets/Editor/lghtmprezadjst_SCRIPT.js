#pragma strict






     
    @MenuItem ("Lightmap/Change Size/256x256")
    static function LMqK () {
        LMSize (256, 256);
    }
     
    @MenuItem ("Lightmap/Change Size/512x512")
    static function LMhK () {
        LMSize (512, 512);
    }
     
    @MenuItem ("Lightmap/Change Size/1024x1024")
    static function LMK () {
        LMSize (1024, 1024);
    }
     
    @MenuItem ("Lightmap/Change Size/2048x2048")
    static function LMDK () {
        LMSize (2048, 2048);
    }
     
    @MenuItem ("Lightmap/Change Size/4096x4096")
    static function LMFK () {
        LMSize (4096, 4096);
    }
     
    static function LMSize (width : float, height : float) {
        Debug.Log ("Lightmap Resolution was: " + LightmapEditorSettings.maxAtlasWidth + "x" + LightmapEditorSettings.maxAtlasHeight + "; and will be: " + width + "x" + height);
        LightmapEditorSettings.maxAtlasWidth = width;
        LightmapEditorSettings.maxAtlasHeight = height;
        Debug.Log ("Lightmap Resolution is: " + LightmapEditorSettings.maxAtlasWidth + "x" + LightmapEditorSettings.maxAtlasHeight + "; and the target was: " + width + "x" + height);
    }
     







function Start () {

}

function Update () {

}