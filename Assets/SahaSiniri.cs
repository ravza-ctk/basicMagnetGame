using UnityEngine;

public class SahaSiniri : MonoBehaviour
{
    public string sahaObjesiAdi = "Tahta";
    public float yaricap = 4.5f;
    public bool oyundaMi = false;

    private Transform sahaMerkezi;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject saha = GameObject.Find(sahaObjesiAdi);
        if (saha != null) { sahaMerkezi = saha.transform; }
    }

    void LateUpdate()
    {
        if (!oyundaMi || sahaMerkezi == null) return;

        Vector3 merkez = new Vector3(sahaMerkezi.position.x, transform.position.y, sahaMerkezi.position.z);
        float mesafe = Vector3.Distance(transform.position, merkez);

        if (mesafe > yaricap)
        {
            Vector3 disariDogruYon = (transform.position - merkez).normalized;
            transform.position = merkez + (disariDogruYon * yaricap);
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }
}