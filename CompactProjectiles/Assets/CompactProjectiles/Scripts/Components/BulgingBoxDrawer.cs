using UnityEngine;

namespace CompactProjectiles
{
    public class BulgingBoxDrawer : MonoBehaviour
    {
        public float BulgeAmount = 1.0f;

        public bool BulgeEachFace = false;

        public float[] FaceBulges = new float[6];

        public int LineCount = 8;

        private BulgingBox _bulgingBox;
        public BulgingBox BulgingBox
        {
            get
            {
                if (_bulgingBox == null)
                {
                    _bulgingBox = new BulgingBox();
                }

                _bulgingBox.Position = transform.position;
                _bulgingBox.Rotation = transform.rotation;
                _bulgingBox.Scale = transform.localScale;
                if (BulgeEachFace)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        _bulgingBox.SetBulge(i, FaceBulges[i]);
                    }
                }
                else
                {
                    _bulgingBox.SetBulge(BulgeAmount);
                }

                return _bulgingBox;
            }
        }

        private Color _gizmosColor = new Color(1, 1, 1, 0.5f);

        private void OnDrawGizmos()
        {
            var box = BulgingBox;
            Gizmos.color = _gizmosColor;
            BulgingBoxUtility.DrawBoxGizmos(box, LineCount);
        }
    }
}
