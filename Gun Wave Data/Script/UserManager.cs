using BackEnd;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    private string fixedPassword = "1234"; // ���� ��й�ȣ
    private string currentUserUUID; // ���� �α����� ������ UUID

    // �̸��� �Է¹޾� ȸ������ �� �α���
    public void SignUpAndLogin(string name)
    {
        var logoutBro = Backend.BMember.Logout();
        if (logoutBro.IsSuccess())
        {
            Debug.Log("�α׾ƿ� ����");
        }
        else
        {
            Debug.LogError($"�α׾ƿ� ����: {logoutBro.GetErrorCode()}, {logoutBro.GetMessage()}");
        }

        // �α���
        BackendReturnObject loginBro = Backend.BMember.CustomLogin(name, fixedPassword);
        if (loginBro.IsSuccess())
        {
            Debug.Log("�α��� ����");
        }
        else
        {
            Debug.Log("���̵� ���� ȸ������ ����");

            // ȸ������
            BackendReturnObject bro = Backend.BMember.CustomSignUp(name, fixedPassword);
            if (bro.IsSuccess())
            {
                Debug.Log("ȸ������ ����");
            }
            else
            {
                Debug.Log($"ȸ������ ����: {bro.GetErrorCode()}, {bro.GetMessage()}");
                return;
            }

            // �г��� ������Ʈ
            BackendReturnObject nicknameBro = Backend.BMember.UpdateNickname(name);
            if (nicknameBro.IsSuccess())
            {
                Debug.Log("�г��� ������Ʈ ����");
            }
            else
            {
                Debug.Log($"�г��� ������Ʈ ����: {nicknameBro.GetErrorCode()}, {nicknameBro.GetMessage()}");
            }
        }
    }
}
