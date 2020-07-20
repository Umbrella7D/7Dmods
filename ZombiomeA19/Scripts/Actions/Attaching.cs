using System;
using System.Xml;
using UnityEngine;

// Token: 0x0200052F RID: 1327
public class Attaching {
	private static int verbose = 0;

	public static void Remove(EntityAlive Self, string pName) {
		if (Self == null) return;
		Transform transform = GameUtils.FindDeepChild(Self.RootTransform, "tempParticle_" + pName, true);
		if (transform == null) return;
		UnityEngine.Object.Destroy(transform.gameObject);
	}

	public static void Apply(EntityAlive Self, string pName) {
		Apply(Self, pName, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
	}
	public static void Apply(EntityAlive Self, string pName, Vector3 local_offset,  Vector3 local_rotation) {

		if (verbose>0) Printer.Print("MyAttachParticleEffectToEntity: Start");
		if (Self == null) return;
		// Transform transform = _params.Transform;
		Transform transform = Self.transform; //Transform;
		if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: Start2");
		// if (!_params.Tags.Test_AnySet(this.usePassedInTransformTag)) {
		if (true) {
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: usePassedInTransformTag");
			transform = Self.transform;
			// if (this.parent_transform_path != null) transform = GameUtils.FindDeepChild(transform, this.parent_transform_path, true);
		} else {
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: use Base Transform");
		}
		if (transform == null) return;

		GameObject goToInstantiate = DataLoader.LoadAsset<GameObject>((pName.IndexOf('#') < 0) ? ("ParticleEffects/" + pName) : pName);

		string text = string.Format("tempParticle_" + goToInstantiate.name, Array.Empty<object>());
		Transform transform2 = transform.Find(text);
		if (transform2 == null) {
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: instatiated");
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(goToInstantiate);
			if (gameObject == null) return;
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: go not null");
			transform2 = gameObject.transform;
			gameObject.name = text;
			Utils.SetLayerRecursively(gameObject, transform.gameObject.layer);
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: rec set");
			transform2.parent = transform;
			transform2.localPosition = local_offset;
			transform2.localRotation = Quaternion.Euler(local_rotation.x, local_rotation.y, local_rotation.z);
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: return - no sound");
			return; // No Audio
			AudioPlayer component = transform2.GetComponent<AudioPlayer>();
			if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: Audio comp");
			if (component != null) component.duration = 100000f;
			ParticleSystem[] componentsInChildren = transform2.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren != null)
			{
				return;
				if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: componentsInChildren");
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (verbose>1) Printer.Print("MyAttachParticleEffectToEntity: children", i, componentsInChildren.Length);
					componentsInChildren[i].Stop();
					// componentsInChildren[i].main.duration = 100000f;
					// Cannot modify the return value of 'UnityEngine.ParticleSystem.main' because it is not a variable
					// Pourtant c'est dans le code de base ...
					// componentsInChildren[i].duration = 100000f; // read only !
					// componentsInChildren[i].main.duration = 100000f;
					componentsInChildren[i].Play();
				}
			}
			if (verbose>0) Printer.Print("MyAttachParticleEffectToEntity: DONE");
		}

	}
}
