using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnPage : MonoBehaviour
{
    //To turn book pages
    private MegaBookBuilder book;
    private int turningPage;
    private float turningDelay;
    private int turningCount;

    // Start is called before the first frame update
    void Start()
    {
        book = FindObjectOfType<MegaBookBuilder>(); //Get alice in wonderland book
        turningDelay = 6.0f;
        turningCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Press left and right keys to turn pages
        if (Input.GetKeyDown("left")){
            //turningPage = -1;
            book.NextPage();
        }
        if (Input.GetKeyDown("right")){
            //turningPage = 1;
            book.PrevPage();
        }
        /*
        if (turningPage > 0)
        {
            turningCount++;
            if (book.GetPage() < 32)
            {
                if (turningCount >= turningDelay)
                {
                    book.NextPage();
                    turningCount = 0;
                }
            }
            else
            {
                turningPage = 0;
                turningCount = 0;
            }
        }
        else if (turningPage < 0)
        {
            turningCount++;
            if (book.GetPage() > 0)
            {
                if (turningCount >= turningDelay)
                {
                    book.PrevPage();
                    turningCount = 0;
                }
            }
            else
            {
                turningPage = 0;
                turningCount = 0;
            }
        }*/
    }
}
