using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class ReplaceBike : ModBehaviour {
    public SkinnedMeshRenderer newSkinnedMeshRenderer;
    public SkinnedMeshRenderer cachedPrevMeshRenderer;
    public Animation ourClips;
    public Animation copiedClips;
}
