using System;

// Token: 0x02000526 RID: 1318
public class MinEventActionAddBuffTyped : MinEventActionBuffModifierBase {
    /* Gives buff based on concrete class of Entity 
    this.buffNames has been parsed by parent class. Use 4 elements:
        player, Z, animal, others
    */

    public override void Execute(MinEventParams _params) {
        MinEventActionTargetedBaseCatcher.Execute(_params, _Execute, GetType());
    }


	public void _Execute(MinEventParams _params) {
		bool netSync = !_params.Self.isEntityRemote | _params.IsLocal;
		int entityId = _params.Self.entityId;

        for (int j = 0; j < this.targets.Count; j++) {
            string buff= "";
            if (this.targets[j] is EntityPlayer) buff = this.buffNames[0];
            else if (this.targets[j] is EntityZombie) buff = this.buffNames[1];
            else if (this.targets[j] is EntityAnimal) buff = this.buffNames[2];
            else buff = this.buffNames[3];
            if (BuffManager.GetBuff(buff) == null) continue;
            this.targets[j].Buffs.AddBuff(buff, entityId, netSync, false);
        }
	}
}
