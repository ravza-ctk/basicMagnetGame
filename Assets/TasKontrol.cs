using UnityEngine;
using Photon.Pun;

public class TasKontrol : MonoBehaviourPun
{
    private Vector3 offset;
    private float zKoor;
    public int tasSahibi;
    public bool sahadaMi = false;
    private bool iznimVarMi = false;
    public AudioClip tutmaSesi;
    public AudioClip yapismaSesi;
    private AudioSource sesCalar;
    private bool sesCikabilirMi = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        sesCalar = GetComponent<AudioSource>();
        Invoke("SesleriAc", 1f);
    }

    void SesleriAc()
    {
        sesCikabilirMi = true;
    }

    [PunRPC]
    public void SetTasSahibi(int no)
    {
        tasSahibi = no;
    }

    private Vector3 FareDunyaPozisyonunuAl()
    {
        Vector3 fareNoktasi = Input.mousePosition;
        fareNoktasi.z = zKoor;
        return Camera.main.ScreenToWorldPoint(fareNoktasi);
    }

    void OnMouseDown()
    {
        if (GameManager.instance.siraDegisiyorMu) return;

        // YENÝ: Sýra bende deðilse taþý tutamam!
        if (GameManager.instance.siradakiOyuncu != tasSahibi) return;

        if (!photonView.IsMine) photonView.RequestOwnership();

        iznimVarMi = true;
        GameManager.instance.sonHamleYapan = tasSahibi; // Bu çok önemliydi unutmadýk!

        rb.isKinematic = false;
        rb.useGravity = false;

        if (sesCalar != null && tutmaSesi != null)
        {
            sesCalar.PlayOneShot(tutmaSesi);
        }

        zKoor = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - FareDunyaPozisyonunuAl();
    }

    void OnMouseDrag()
    {
        if (!iznimVarMi) return;
        if (!photonView.IsMine) return;

        // YENÝ: Sýra bende deðilse sürüklemeyi býrak
        if (GameManager.instance.siraDegisiyorMu || GameManager.instance.siradakiOyuncu != tasSahibi)
        {
            iznimVarMi = false;
            return;
        }

        transform.position = FareDunyaPozisyonunuAl() + offset;
    }

    void OnMouseUp()
    {
        if (!iznimVarMi) return;
        iznimVarMi = false;

        rb.isKinematic = true;

        GetComponent<SahaSiniri>().oyundaMi = true;
        if (!sahadaMi)
        {
            sahadaMi = true;
            GameManager.instance.HamleYapildi(tasSahibi, true);
        }
        else
        {
            GameManager.instance.HamleYapildi(tasSahibi, false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (sahadaMi && sesCikabilirMi && collision.gameObject.name.Contains("tas"))
        {
            if (sesCalar != null && yapismaSesi != null)
            {
                sesCalar.PlayOneShot(yapismaSesi);
            }
        }

        if (!sahadaMi || !iznimVarMi) return;

        if (collision.gameObject.name.Contains("tas"))
        {
            TasKontrol diger = collision.gameObject.GetComponent<TasKontrol>();
            if (diger != null && diger.sahadaMi)
            {
                CezayiUygula();
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!sahadaMi || !photonView.IsMine) return;

        if (collision.gameObject.name.Contains("tas"))
        {
            TasKontrol digerTas = collision.gameObject.GetComponent<TasKontrol>();

            if (digerTas != null && digerTas.sahadaMi)
            {
                SahaSiniri benimSinir = GetComponent<SahaSiniri>();
                SahaSiniri digerSinir = collision.gameObject.GetComponent<SahaSiniri>();

                if (benimSinir != null && digerSinir != null && benimSinir.oyundaMi && digerSinir.oyundaMi)
                {
                    CezayiUygula();
                }
            }
        }
    }

    void CezayiUygula()
    {
        Collider[] yakindakiTumObjeler = Physics.OverlapSphere(transform.position, 1.0f);
        foreach (Collider obje in yakindakiTumObjeler)
        {
            if (obje.gameObject.name.Contains("tas"))
            {
                SahaSiniri oSinir = obje.GetComponent<SahaSiniri>();
                if (oSinir != null && oSinir.oyundaMi)
                {
                    GameManager.instance.TasiGeriGonder(obje.gameObject);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            if (rb != null) rb.isKinematic = true;
            return;
        }

        if (rb != null) rb.isKinematic = false;

        Vector3 mevcutPozisyon = transform.position;
        mevcutPozisyon.x = Mathf.Clamp(mevcutPozisyon.x, -13f, 13f);
        mevcutPozisyon.z = Mathf.Clamp(mevcutPozisyon.z, -6.5f, 6.5f);
        transform.position = mevcutPozisyon;
    }
}