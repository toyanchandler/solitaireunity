using UnityEngine;
namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    [RequireComponent(typeof(MeshRenderer))]
    public class OffsetChanger : MonoBehaviour
    {
        [Tooltip("Speed at which the Y offset will change")]
        public float speed = 1.0f;

        private MeshRenderer rend;
        private Material mat;
        private static readonly int Base = Shader.PropertyToID("_BaseMap");

        private void Start()
        {
            rend = GetComponent<MeshRenderer>();
            mat=rend.materials[0];
        }

        private void Update()
        {
            float yOffset = mat.GetTextureOffset(Base).y - speed * Time.deltaTime;
            mat.SetTextureOffset(Base, new Vector2(0, yOffset));
        }
    }
}
