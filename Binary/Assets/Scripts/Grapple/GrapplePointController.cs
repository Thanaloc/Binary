using UnityEngine;
using UnityEngine.UI;

public class GrapplePointController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _UI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHideUI(true);
            PlayerEventsReceiver.Instance?.GrapplePointEvent(true, _UI.transform);//todo
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowHideUI(false);
            PlayerEventsReceiver.Instance?.GrapplePointEvent(false, null);
        }
    }

    private void ShowHideUI(bool p_show)
    {
        _UI.enabled = p_show;
    }
}
