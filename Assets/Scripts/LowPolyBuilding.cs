using UnityEngine;

public class LowPolyBuilding : MonoBehaviour
{
    public float unit = 1f;

    void Start()
    {
        BuildGround();
        BuildStands();
        BuildLines();
        BuildRope();
    }

    void BuildGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.transform.localScale = new Vector3(20 * unit, 0.5f * unit, 5 * unit);
        ground.transform.position = new Vector3(0, -0.25f * unit, 0);
        ground.GetComponent<Renderer>().material.color = Color.green;
    }

    void BuildStands()
    {
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.transform.localScale = new Vector3(20 * unit, 2 * unit, 1 * unit);
            stand.transform.position = new Vector3(0, 1 * unit, side * 3 * unit);
            stand.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    void BuildLines()
    {
        // Center line
        GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centerLine.transform.localScale = new Vector3(0.1f * unit, 0.01f * unit, 5 * unit);
        centerLine.transform.position = new Vector3(0, 0.01f, 0);
        centerLine.GetComponent<Renderer>().material.color = Color.white;

        // End lines
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject endLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            endLine.transform.localScale = new Vector3(0.1f * unit, 0.01f * unit, 5 * unit);
            endLine.transform.position = new Vector3(side * 8f * unit, 0.01f, 0);
            endLine.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void BuildRope()
    {
        int segments = 5;
        float ropeLength = 8f * unit;
        for (int i = 0; i < segments; i++)
        {
            GameObject ropePart = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ropePart.transform.localScale = new Vector3(0.2f * unit, ropeLength / segments / 2, 0.2f * unit);
            ropePart.transform.rotation = Quaternion.Euler(90, 0, 0);
            float xPos = -ropeLength / 2 + i * (ropeLength / (segments - 1));
            ropePart.transform.position = new Vector3(xPos, 0.1f * unit, 0);
            ropePart.GetComponent<Renderer>().material.color = new Color(0.6f, 0.3f, 0.1f);
        }
    }
}
