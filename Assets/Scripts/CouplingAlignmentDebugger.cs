using UnityEngine;

public class CouplingAlignmentDebugger : MonoBehaviour
{
    public Transform maleFace;
    public Transform maleConnectedAnchor;

    void Update()
    {
        if (!maleFace || !maleConnectedAnchor) return;

        // Direction each Z+ is facing (world space)
        Vector3 maleFaceForward = maleFace.forward;
        Vector3 anchorForward = maleConnectedAnchor.forward;

        // The vector from face to anchor
        Vector3 faceToAnchor = (maleConnectedAnchor.position - maleFace.position).normalized;

        // Check dot products
        float faceDot = Vector3.Dot(maleFaceForward, faceToAnchor);
        float anchorDot = Vector3.Dot(anchorForward, -faceToAnchor);

        Debug.Log($"Face→Anchor dot: {faceDot:F2}, Anchor→Face dot: {anchorDot:F2}");

        // Rough guidance:
        // Both should be close to +1.00 if perfectly facing each other.
        // If either is negative, one of them is flipped 180°.
    }
}
