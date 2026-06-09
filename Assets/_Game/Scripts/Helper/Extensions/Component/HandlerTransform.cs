using System.Collections.Generic;
using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerTransform
    {
        #region Position Extensions
        public static void SetPositionX(this Transform self, float x) => self.position = new Vector3(x, self.position.y, self.position.z);
        public static void SetPositionY(this Transform self, float y) => self.position = new Vector3(self.position.x, y, self.position.z);
        public static void SetPositionZ(this Transform self, float z) => self.position = new Vector3(self.position.x, self.position.y, z);
        public static void SetPosition(this Transform self, float x, float y, float z) => self.position = new Vector3(x, y, z);

        public static void AddPositionX(this Transform self, float x) => self.SetPositionX(x + self.position.x);

        public static void AddPositionY(this Transform self, float y) => self.SetPositionY(y + self.position.y);

        public static void AddPositionZ(this Transform self, float z) => self.SetPositionZ(z + self.position.z);
        
        public static void AddPosition(this Transform self, float x, float y, float z) => self.SetPosition(self.position.x + x, self.position.y + y, self.position.z + z);
        public static void SetLocalPositionX(this Transform self, float x) => self.localPosition = new Vector3(x, self.localPosition.y, self.localPosition.z);
        public static void SetLocalPositionY(this Transform self, float y) => self.localPosition = new Vector3(self.localPosition.x, y, self.localPosition.z);
        public static void SetLocalPositionZ(this Transform self, float z) => self.localPosition = new Vector3(self.localPosition.x, self.localPosition.y, z);

        public static void SetLocalPosition(this Transform self, float x, float y, float z) => self.localPosition = new Vector3(x, y, z);
        
        public static void AddLocalPositionX(this Transform self, float x) => self.SetLocalPositionX(x + self.localPosition.x);

        public static void AddLocalPositionY(this Transform self, float y) => self.SetLocalPositionY(y + self.localPosition.y);

        public static void AddLocalPositionZ(this Transform self, float z) => self.SetLocalPositionZ(z + self.localPosition.z);
        
        public static void AddLocalPosition(this Transform self, float x, float y, float z) => self.SetLocalPosition(self.localPosition.x + x, self.localPosition.y + y, self.localPosition.z + z);
        
        public static void SetAnchoredPositionX(this RectTransform self, float x) => self.anchoredPosition = new Vector2(x, self.anchoredPosition.y);
        
        public static void SetAnchoredPositionY(this RectTransform self, float y) => self.anchoredPosition = new Vector2(self.anchoredPosition.x, y);
        
        public static void SetAnchoredPositionZ(this RectTransform self, float z) => self.anchoredPosition = new Vector2(self.anchoredPosition.x, z);
        
        #endregion

        #region Rotational Extensions
        public static void SetEulerAngleX(this Transform self, float x) => self.eulerAngles = new Vector3(x, self.eulerAngles.y, self.eulerAngles.z);
        public static void SetEulerAngleY(this Transform self, float y) => self.eulerAngles = new Vector3(self.eulerAngles.x, y, self.eulerAngles.z);
        public static void SetEulerAngleZ(this Transform self, float z) => self.eulerAngles = new Vector3(self.eulerAngles.x, self.eulerAngles.y, z);
        public static void SetEulerAngle(this Transform self, float x, float y, float z) => self.eulerAngles = new Vector3(x, y, z);

        public static void AddEulerAngleX(this Transform self, float x) => self.SetEulerAngleX(self.eulerAngles.x + x);

        public static void AddEulerAngleY(this Transform self, float y) => self.SetEulerAngleY(self.eulerAngles.y + y);

        public static void AddEulerAngleZ(this Transform self, float z) => self.SetEulerAngleZ(self.eulerAngles.z + z);
        
        public static void AddEulerAngle(this Transform self, float x, float y, float z) => self.SetEulerAngle(self.eulerAngles.x + x, self.eulerAngles.y + y, self.eulerAngles.z + z);
        public static void SetLocalEulerAngleX(this Transform self, float x) => self.localEulerAngles = new Vector3(x, self.localEulerAngles.y, self.localEulerAngles.z);
        public static void SetLocalEulerAngleY(this Transform self, float y) => self.localEulerAngles = new Vector3(self.localEulerAngles.x, y, self.localEulerAngles.z);
        public static void SetLocalEulerAngleZ(this Transform self, float z) => self.localEulerAngles = new Vector3(self.localEulerAngles.x, self.localEulerAngles.y, z);
        
        public static void SetLocalEulerAngle(this Transform self, float x, float y, float z) => self.localEulerAngles = new Vector3(x, y, z);

        public static void AddLocalEulerAngleX(this Transform self, float x) => self.SetLocalEulerAngleX(self.localEulerAngles.x + x);

        public static void AddLocalEulerAngleY(this Transform self, float y) => self.SetLocalEulerAngleY(self.localEulerAngles.y + y);

        public static void AddLocalEulerAngleZ(this Transform self, float z) => self.SetLocalEulerAngleZ(self.localEulerAngles.z + z);
        
        public static void AddLocalEulerAngle(this Transform self, float x, float y, float z) => self.SetLocalEulerAngle(self.localEulerAngles.x + x, self.localEulerAngles.y + y, self.localEulerAngles.z + z);

        #endregion

        #region Scale Extensions
        public static void SetLocalScaleX(this Transform self, float x) => self.localScale = new Vector3(x, self.localScale.y, self.localScale.z);
        public static void SetLocalScaleY(this Transform self, float y) => self.localScale = new Vector3(self.localScale.x, y, self.localScale.z);
        public static void SetLocalScaleZ(this Transform self, float z) => self.localScale = new Vector3(self.localScale.x, self.localScale.y, z);
        public static void SetLocalScale(this Transform self, float x, float y, float z) => self.localScale = new Vector3(x, y, z);

        public static void AddLocalScaleX(this Transform self, float x) => self.SetLocalScaleX(self.localScale.x + x);

        public static void AddLocalScaleY(this Transform self, float y) => self.SetLocalScaleY(self.localScale.y + y);

        public static void AddLocalScaleZ(this Transform self, float z) => self.SetLocalScaleZ(self.localScale.z + z);
        
        public static void AddLocalScale(this Transform self, float x, float y, float z) => self.SetLocalScale(self.localScale.x + x, self.localScale.y + y, self.localScale.z + z);

        #endregion
       

        public static void ResetTransform(this Transform self)
        {
            self.localPosition = Vector3.zero;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.identity;
        }
        

        public static List<Transform> GetChildren(this Transform parent)
        {
            List<Transform> children = new List<Transform>();
            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                children.Add(current);

                int childCount = current.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = current.GetChild(i);
                    queue.Enqueue(child);
                }
            }

            return children;
        }
        
        public static Transform GetChildWithIndex(this Transform parent, int index)
        {
            List<Transform> children = parent.GetChildren();
            return children[index];
        }
        
        public static void DestroyChildren(this Transform parent)
        {
            List<Transform> children = parent.GetChildren();
            for (int i = children.Count - 1; i >= 0; i--)
            {
                Object.Destroy(children[i].gameObject);
            }
        }
        
        public static void DestroyChildWithIndex(this Transform parent, int index)
        {
            List<Transform> children = parent.GetChildren();
            Object.Destroy(children[index].gameObject);
        }
        
        public static void AddChildToEnd(this Transform parent, Transform child)
        {
            child.SetParent(parent);
            child.SetAsLastSibling();
        }
        
        public static int GetActiveChildrenCount(this Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }
        
        public static int GetDisabledChildrenCount(this Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (!parent.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }
        
        public static void LookAtCamera(this Transform self, Quaternion rotation)
        {
            self.LookAt(self.position + rotation * Vector3.forward,
                rotation * Vector3.up);
        }
    }
}