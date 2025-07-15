using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Pig : WeakAnimal
{
    protected override void ReSetting()
    {
        base.ReSetting();
        RandomAction();
    }


    private void Wait()
    {
        currentTime = waitTime;
        Debug.Log("���");
    }


    private void Eat()
    {
        currentTime = waitTime;
        anim.SetTrigger("Eat");
        Debug.Log("Ǯ���");
    }


    private void Peek()
    {
        currentTime = waitTime;
        anim.SetTrigger("Peek");
        Debug.Log("�θ���");
    }


    private void RandomAction()
    {
        RandomSound();
        int _random = Random.Range(0, 4); // ���, Ǯ���, �θ���, �ȱ�

        if (_random == 0)
            Wait();
        else if (_random == 1)
            Eat();
        else if (_random == 2)
            Peek();
        else if (_random == 3)
            TryWalk();
    }
}
