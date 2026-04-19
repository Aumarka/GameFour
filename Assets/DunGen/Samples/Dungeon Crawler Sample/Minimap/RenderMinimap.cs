using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Renders the minimap to a RawImage on a GameObject named "Minimap"
	/// and the icons to a RawImage on a GameObject named "Minimap Icons"
	/// </summary>
	[RequireComponent(typeof(Camera))]
	sealed class RenderMinimap : MonoBehaviour
	{
		[SerializeField] private Material drawMinimapMaterial = null;
		[SerializeField] private Camera minimapIconsCamera = null;
		[SerializeField] private Material createDistanceFieldMaterial = null;

		private RawImage minimapImage = null;
		private RawImage minimapIconsImage = null;
		private Camera minimapCamera;
		private RenderTexture cameraBuffer;
		private RenderTexture distanceFieldBuffer;
		private RenderTexture outputBuffer;
		private RenderTexture iconsBuffer;
		private Material createDistanceFieldMaterialInstance;

		private void OnEnable()
		{
			minimapCamera = GetComponent<Camera>();

			const int inputRes = 512;
			const int outputRes = 512;
			const int depthBits = 24;

			cameraBuffer = new RenderTexture(inputRes, inputRes, depthBits);
			distanceFieldBuffer = new RenderTexture(inputRes, inputRes, 0);
			outputBuffer = new RenderTexture(outputRes, outputRes, 0);
			iconsBuffer = new RenderTexture(outputRes, outputRes, depthBits);

			// Setup material
			createDistanceFieldMaterialInstance = new Material(createDistanceFieldMaterial);
			createDistanceFieldMaterialInstance.SetFloat("_TextureSize", inputRes);

			// Tell the minimap camera to render into an off-screen buffer
			minimapCamera.targetTexture = cameraBuffer;

			// Hook the output buffer up to the RawImage component in the UI
			if (minimapIconsCamera != null)
				minimapIconsCamera.targetTexture = iconsBuffer;

			if (minimapImage == null)
				minimapImage = GameObject.Find("Minimap").GetComponent<RawImage>();

			if (minimapIconsImage == null)
				minimapIconsImage = GameObject.Find("Minimap Icons").GetComponent<RawImage>();

			minimapImage.texture = outputBuffer;
			minimapIconsImage.texture = iconsBuffer;

			RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
		}

		private void OnDisable()
		{
			RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

			if (minimapCamera != null)
				minimapCamera.targetTexture = null;

			if (minimapIconsCamera != null)
				minimapIconsCamera.targetTexture = null;

			Destroy(cameraBuffer);
			Destroy(distanceFieldBuffer);
			Destroy(outputBuffer);
			Destroy(iconsBuffer);
			Destroy(createDistanceFieldMaterialInstance);

			cameraBuffer = null;
			distanceFieldBuffer = null;
			outputBuffer = null;
			iconsBuffer = null;
			createDistanceFieldMaterialInstance = null;
		}

		private void OnPostRender()
		{
			// Built-in Render Pipeline
			if (GraphicsSettings.currentRenderPipeline == null)
				ProcessMinimap();
		}

		private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
		{
			// Scriptable Render Pipelines
			if (GraphicsSettings.currentRenderPipeline != null && camera == minimapCamera)
				ProcessMinimap();
		}

		private void ProcessMinimap()
		{
			if (cameraBuffer == null || distanceFieldBuffer == null || outputBuffer == null)
				return;

			// After the minimap camera is done rendering, convert the contents to a distance field
			Graphics.Blit(cameraBuffer, distanceFieldBuffer, createDistanceFieldMaterialInstance);

			// Render the distance field as the final minimap
			Graphics.Blit(distanceFieldBuffer, outputBuffer, drawMinimapMaterial);
		}
	}
}