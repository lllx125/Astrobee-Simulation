using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class KeyBoardControl : MonoBehaviour
{

    Astrobee astrobee;
    // Dictionary mapping key inputs to vent activations
    Dictionary<KeyCode, int[]> keyDict = new Dictionary<KeyCode, int[]>
    {
        { KeyCode.A, Parameters.CONTROL["+x"] },   // +x
        { KeyCode.D, Parameters.CONTROL["-x"] },   // -x
        { KeyCode.W, Parameters.CONTROL["-y"] },   // -y
        { KeyCode.S, Parameters.CONTROL["+y"] },   // +y
        { KeyCode.Q, Parameters.CONTROL["+z"] },   // +z
        { KeyCode.E, Parameters.CONTROL["-z"] },   // -z
        { KeyCode.I, Parameters.CONTROL["+Rx1"] },  // +Rx
        { KeyCode.K, Parameters.CONTROL["-Rx1"] },  // -Rx
        { KeyCode.J, Parameters.CONTROL["+Ry1"] },  // +Ry
        { KeyCode.L, Parameters.CONTROL["-Ry1"] },  // -Ry
        { KeyCode.U, Parameters.CONTROL["+Rz1"] },  // +Rz
        { KeyCode.O, Parameters.CONTROL["-Rz1"] },   // -Rz
        { KeyCode.Alpha1, new int[] { 1 } },  // vent 1
        { KeyCode.Alpha2, new int[] { 2 } },  // vent 2
        { KeyCode.Alpha3, new int[] { 3 } },  // vent 3
        { KeyCode.Alpha4, new int[] { 4 } },  // vent 4
        { KeyCode.Alpha5, new int[] { 5 } },  // vent 5
        { KeyCode.Alpha6, new int[] { 6 } },  // vent 6
        { KeyCode.Alpha7, new int[] { 7 } },  // vent 7
        { KeyCode.Alpha8, new int[] { 8 } },  // vent 8
        { KeyCode.Alpha9, new int[] { 9 } },  // vent 9
        { KeyCode.Alpha0, new int[] { 10 } }, // vent 10
        { KeyCode.Minus, new int[] { 11 } },  // vent 11
        { KeyCode.Equals, new int[] { 12 } }  // vent 12
    };


    void Start()
    {
        // Look for the Astrobee component in this GameObject
        astrobee = GetComponent<Astrobee>();
        // Check if the component was found
        if (astrobee == null)
        {
            Debug.LogError("Astrobee component not found on this GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Iterate through each key-value pair in the dictionary
        foreach (var entry in keyDict)
        {
            // Check if the key was just pressed
            if (Input.GetKeyDown(entry.Key))
            {
                // Open all vents in the value array
                foreach (int ventIndex in entry.Value)
                {
                    astrobee.OpenVent(ventIndex);
                }
            }
            // Check if the key was just released
            else if (Input.GetKeyUp(entry.Key))
            {
                // Close all vents in the value array
                foreach (int ventIndex in entry.Value)
                {
                    astrobee.CloseVent(ventIndex);
                }
            }
        }
    }


}
