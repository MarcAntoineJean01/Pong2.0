using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace TMPro.Examples
{
[ExecuteInEditMode]
    public class SkewTextExample : MonoBehaviour
    {
	public float radius = 0.5f;
	public float wrapAngle = 360.0f;
	public float scaleFactor = 100.0f;
    	private float circumference
	{
		get
		{
			if(_radius != radius || _scaleFactor != scaleFactor)
			{
				_circumference = 2.0f*Mathf.PI*radius*scaleFactor;
				_radius = radius;
				_scaleFactor = scaleFactor;
			}

			return _circumference;
		}
	}
	private float _radius = -1;
	private float _scaleFactor = -1;
	private float _circumference = -1;
        private TMP_Text m_TextComponent;

        public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
        //public float AngleMultiplier = 1.0f;
        //public float SpeedMultiplier = 1.0f;
        public float CurveScale = 1.0f;
        public float ShearAmount = 1.0f;

        void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
            		if(radius <= 0.0f)
		{
			radius = 0.001f;
		}
		if(scaleFactor <= 0.0f)
		{
			scaleFactor = 0.001f;
		}
        }


        void OnEnable()
        {
            // StartCoroutine(WarpTextZ());
        
            List<UIVertex> stream = new List<UIVertex>();


            for (int i = 0; i < stream.Count; i++)
            {
                UIVertex v = stream[i];

                float percentCircumference = v.position.x/circumference;
                Vector3 offset = Quaternion.Euler(0.0f,0.0f,-percentCircumference*360.0f)*Vector3.up;
                v.position = offset*radius*scaleFactor + offset*v.position.y;
                v.position += Vector3.down*radius*scaleFactor;

                stream[i] = v;
            }

            testwarp();
        }


        private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
        {
            AnimationCurve newCurve = new AnimationCurve();

            newCurve.keys = curve.keys;

            return newCurve;
        }


        /// <summary>
        ///  Method to curve text along a Unity animation curve.
        /// </summary>
        /// <param name="textComponent"></param>
        /// <returns></returns>
        /// 
        /// 
        /// 
        /// 
        /// 
        void testwarp()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            Vector3[] vertices;
            Matrix4x4 matrix;

            m_TextComponent.havePropertiesChanged = true; // Need to force the TextMeshPro Object to be updated.

            m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            int characterCount = textInfo.characterCount;

            float boundsMinX = m_TextComponent.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
            float boundsMaxX = m_TextComponent.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;
            float boundsMinY = m_TextComponent.bounds.min.y;  //textInfo.meshInfo[0].mesh.bounds.min.x;
            float boundsMaxY = m_TextComponent.bounds.max.y;  //textInfo.meshInfo[0].mesh.bounds.max.x;
            for (int i = 0; i < characterCount; i++)
            {
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the index of the mesh used by this character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                vertices = textInfo.meshInfo[materialIndex].vertices;

                // Compute the baseline mid point for each character
                Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                // Apply offset to adjust our pivot point.
                vertices[vertexIndex + 0] += -offsetToMidBaseline;
                vertices[vertexIndex + 1] += -offsetToMidBaseline;
                vertices[vertexIndex + 2] += -offsetToMidBaseline;
                vertices[vertexIndex + 3] += -offsetToMidBaseline;

                // Apply the Shearing FX
                Vector3 topShear = new Vector3(ShearAmount * (textInfo.characterInfo[i].topRight.y - textInfo.characterInfo[i].baseLine), 0, 0);
                Vector3 bottomShear = new Vector3(ShearAmount * (textInfo.characterInfo[i].baseLine - textInfo.characterInfo[i].bottomRight.y), 0, 0);

                vertices[vertexIndex + 0] += -bottomShear;
                vertices[vertexIndex + 1] += topShear;
                vertices[vertexIndex + 2] += topShear;
                vertices[vertexIndex + 3] += -bottomShear;


                // Compute the angle of rotation for each character based on the animation curve
                float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                float x1 = x0 + 0.0001f;
                float y0 = (offsetToMidBaseline.y - boundsMinY) / (boundsMaxY - boundsMinY); // Character's position relative to the bounds of the mesh.
                float y1 = y0 + 0.0001f;
                float z0 = VertexCurve.Evaluate(y0) * CurveScale;
                float z1 = VertexCurve.Evaluate(y1) * CurveScale;

                Vector3 horizontal = new Vector3(1, 0, 0);
                //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                Vector3 tangent = new Vector3(0, y1 * (boundsMaxY - boundsMinY) + boundsMinY, z1) - new Vector3(0, offsetToMidBaseline.y, z0);

                float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                Vector3 cross = Vector3.Cross(horizontal, tangent);
                float angle = cross.y > 0 ? dot : 360 - dot;

                matrix = Matrix4x4.TRS(new Vector3(0, 0, z0), Quaternion.Euler(0, 0, angle), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                vertices[vertexIndex + 0] += offsetToMidBaseline;
                vertices[vertexIndex + 1] += offsetToMidBaseline;
                vertices[vertexIndex + 2] += offsetToMidBaseline;
                vertices[vertexIndex + 3] += offsetToMidBaseline;
            }

            m_TextComponent.UpdateVertexData();
        }
        /// 
        /// 
        IEnumerator WarpText()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            //Mesh mesh = m_TextComponent.textInfo.meshInfo[0].mesh;

            Vector3[] vertices;
            Matrix4x4 matrix;

            m_TextComponent.havePropertiesChanged = true; // Need to force the TextMeshPro Object to be updated.
            CurveScale *= 10;
            float old_CurveScale = CurveScale;
            float old_ShearValue = ShearAmount;
            AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);

            while (true)
            {
                if (!m_TextComponent.havePropertiesChanged && old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value && old_ShearValue == ShearAmount)
                {
                    yield return null;
                    continue;
                }

                old_CurveScale = CurveScale;
                old_curve = CopyAnimationCurve(VertexCurve);
                old_ShearValue = ShearAmount;

                m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

                TMP_TextInfo textInfo = m_TextComponent.textInfo;
                int characterCount = textInfo.characterCount;


                if (characterCount == 0) continue;

                //vertices = textInfo.meshInfo[0].vertices;
                //int lastVertexIndex = textInfo.characterInfo[characterCount - 1].vertexIndex;

                float boundsMinX = m_TextComponent.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
                float boundsMaxX = m_TextComponent.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;



                for (int i = 0; i < characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Get the index of the mesh used by this character.
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    vertices = textInfo.meshInfo[materialIndex].vertices;

                    // Compute the baseline mid point for each character
                    Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                    //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                    // Apply offset to adjust our pivot point.
                    vertices[vertexIndex + 0] += -offsetToMidBaseline;
                    vertices[vertexIndex + 1] += -offsetToMidBaseline;
                    vertices[vertexIndex + 2] += -offsetToMidBaseline;
                    vertices[vertexIndex + 3] += -offsetToMidBaseline;

                    // Apply the Shearing FX
                    float shear_value = ShearAmount * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * (textInfo.characterInfo[i].topRight.y - textInfo.characterInfo[i].baseLine), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (textInfo.characterInfo[i].baseLine - textInfo.characterInfo[i].bottomRight.y), 0, 0);

                    vertices[vertexIndex + 0] += -bottomShear;
                    vertices[vertexIndex + 1] += topShear;
                    vertices[vertexIndex + 2] += topShear;
                    vertices[vertexIndex + 3] += -bottomShear;


                    // Compute the angle of rotation for each character based on the animation curve
                    float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                    float x1 = x0 + 0.0001f;
                    float y0 = VertexCurve.Evaluate(x0) * CurveScale;
                    float y1 = VertexCurve.Evaluate(x1) * CurveScale;

                    Vector3 horizontal = new Vector3(1, 0, 0);
                    //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                    Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

                    float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                    Vector3 cross = Vector3.Cross(horizontal, tangent);
                    float angle = cross.z > 0 ? dot : 360 - dot;

                    matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                    vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    vertices[vertexIndex + 0] += offsetToMidBaseline;
                    vertices[vertexIndex + 1] += offsetToMidBaseline;
                    vertices[vertexIndex + 2] += offsetToMidBaseline;
                    vertices[vertexIndex + 3] += offsetToMidBaseline;
                }


                // Upload the mesh with the revised information
                m_TextComponent.UpdateVertexData();

                yield return null; // new WaitForSeconds(0.025f);
            }
        }
        IEnumerator WarpTextZ()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;


            Vector3[] vertices;
            Matrix4x4 matrix;

            m_TextComponent.havePropertiesChanged = true;
            CurveScale *= 10;
            float old_CurveScale = CurveScale;
            float old_ShearValue = ShearAmount;
            AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);

            while (true)
            {
                if (!m_TextComponent.havePropertiesChanged && old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value && old_ShearValue == ShearAmount)
                {
                    yield return null;
                    continue;
                }

                old_CurveScale = CurveScale;
                old_curve = CopyAnimationCurve(VertexCurve);
                old_ShearValue = ShearAmount;

                m_TextComponent.ForceMeshUpdate();

                TMP_TextInfo textInfo = m_TextComponent.textInfo;
                int characterCount = textInfo.characterCount;


                if (characterCount == 0) continue;


                float boundsMinX = m_TextComponent.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
                float boundsMaxX = m_TextComponent.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;
                float boundsMinY = m_TextComponent.bounds.min.y;  //textInfo.meshInfo[0].mesh.bounds.min.y;
                float boundsMaxY = m_TextComponent.bounds.max.y;  //textInfo.meshInfo[0].mesh.bounds.max.y;


                for (int i = 0; i < characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    vertices = textInfo.meshInfo[materialIndex].vertices;

                    Vector3 offsetToMidBaseline = new Vector3((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, (vertices[vertexIndex + 0].y + vertices[vertexIndex + 2].y) / 2, textInfo.characterInfo[i].baseLine);

                    vertices[vertexIndex + 0] += -offsetToMidBaseline;
                    vertices[vertexIndex + 1] += -offsetToMidBaseline;
                    vertices[vertexIndex + 2] += -offsetToMidBaseline;
                    vertices[vertexIndex + 3] += -offsetToMidBaseline;

                    float shear_value = ShearAmount * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * (textInfo.characterInfo[i].topRight.z - textInfo.characterInfo[i].baseLine), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (textInfo.characterInfo[i].baseLine - textInfo.characterInfo[i].bottomRight.z), 0, 0);

                    vertices[vertexIndex + 0] += -bottomShear;
                    vertices[vertexIndex + 1] += topShear;
                    vertices[vertexIndex + 2] += topShear;
                    vertices[vertexIndex + 3] += -bottomShear;


                    // Compute the angle of rotation for each character based on the animation curve
                    float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                    float x1 = x0 + 0.0001f;
                    float y0 = (offsetToMidBaseline.x - boundsMinY) / (boundsMaxY - boundsMinY); // Character's position relative to the bounds of the mesh.
                    float y1 = x0 + 0.0001f;
                    float z0 = VertexCurve.Evaluate(x0) * CurveScale;
                    float z1 = VertexCurve.Evaluate(x1) * CurveScale;

                    Vector3 horizontal = new Vector3(1, 0, 0);
                    //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                    Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1, z1) - new Vector3(offsetToMidBaseline.x, y0, z0);

                    float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                    Vector3 cross = Vector3.Cross(horizontal, tangent);
                    float angle = cross.z > 0 ? dot : 360 - dot;

                    matrix = Matrix4x4.TRS(new Vector3(0, y0, z0), Quaternion.Euler(0, 0, angle), Vector3.one);

                    vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    vertices[vertexIndex + 0] += offsetToMidBaseline;
                    vertices[vertexIndex + 1] += offsetToMidBaseline;
                    vertices[vertexIndex + 2] += offsetToMidBaseline;
                    vertices[vertexIndex + 3] += offsetToMidBaseline;
                }


                // Upload the mesh with the revised information
                m_TextComponent.UpdateVertexData();

                yield return null; // new WaitForSeconds(0.025f);
            }
        }
    }
}
