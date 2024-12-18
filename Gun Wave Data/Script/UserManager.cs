using BackEnd;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    private string fixedPassword = "1234"; // 고정 비밀번호
    private string currentUserUUID; // 현재 로그인한 유저의 UUID

    // 이름을 입력받아 회원가입 및 로그인
    public void SignUpAndLogin(string name)
    {
        var logoutBro = Backend.BMember.Logout();
        if (logoutBro.IsSuccess())
        {
            Debug.Log("로그아웃 성공");
        }
        else
        {
            Debug.LogError($"로그아웃 실패: {logoutBro.GetErrorCode()}, {logoutBro.GetMessage()}");
        }

        // 로그인
        BackendReturnObject loginBro = Backend.BMember.CustomLogin(name, fixedPassword);
        if (loginBro.IsSuccess())
        {
            Debug.Log("로그인 성공");
        }
        else
        {
            Debug.Log("아이디가 없음 회원가입 진행");

            // 회원가입
            BackendReturnObject bro = Backend.BMember.CustomSignUp(name, fixedPassword);
            if (bro.IsSuccess())
            {
                Debug.Log("회원가입 성공");
            }
            else
            {
                Debug.Log($"회원가입 실패: {bro.GetErrorCode()}, {bro.GetMessage()}");
                return;
            }

            // 닉네임 업데이트
            BackendReturnObject nicknameBro = Backend.BMember.UpdateNickname(name);
            if (nicknameBro.IsSuccess())
            {
                Debug.Log("닉네임 업데이트 성공");
            }
            else
            {
                Debug.Log($"닉네임 업데이트 실패: {nicknameBro.GetErrorCode()}, {nicknameBro.GetMessage()}");
            }
        }
    }
}
