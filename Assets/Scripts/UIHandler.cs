using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    private Button btnTest;

    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();

        btnTest = document.rootVisualElement
            .Q<Button>("buttonTest");

        btnTest.clicked += OnTestClicked;
    }

    private void OnDisable()
    {
        if (btnTest != null)
        {
            btnTest.clicked -= OnTestClicked;
        }
    }

    private void OnTestClicked()
    {
        Debug.Log("test");
    }
}
