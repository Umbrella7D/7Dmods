using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;


using SdtdUtils;
using SdtdUtils.Blocks;

namespace SdtdUtils {

public class OptionEffect {
    public OptionEffect Copy() {
        OptionEffect copied = this.MemberwiseClone() as OptionEffect;
        copied.OptionBlock = this.OptionBlock.Copy() as BlockSetter.Options;
        copied.OptionItem = this.OptionItem.Copy() as EffectsItem.Options;
        copied.OptionEntity = this.OptionEntity.Copy() as EntityCreation.Options;
        copied.OptionShape = this.OptionShape.Copy() as EffectsGround.Options;
        return copied;
    }
    public string at = "";
    public string th = "";
    public string Klass = "None";
    public string Effect = "None";
    public BlockSetter.Options OptionBlock = new BlockSetter.Options();

    public EffectsItem.Options OptionItem = new EffectsItem.Options();

    public EntityCreation.Options OptionEntity = new EntityCreation.Options();

    public EffectsGround.Options OptionShape = new EffectsGround.Options();

}


////////////////////
} // End Namespace
////////////////////