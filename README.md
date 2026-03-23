# Unity_VSurv

유니티를 이용하여 뱀파이어 서바이벌 류 게임을 제작하고 있습니다.


[현재 유니티 쪽 구현 상태] (VSurvival 폴더)
1. 게임 흐름
- 로비 화면
- 시작 버튼으로 게임 시작
- 게임 오버 시 다시 로비로 복귀하는 구조
- GameManager 중심으로 Lobby / Playing 상태 기반 UI 제어 정리 중

2. 플레이어 / 전투 / 적 / 성장
- 플레이어 WASD 이동
- 적 3종 가중치 랜덤 스폰
- 자동 발사
- ProjectilePool 기반 총알 풀링
- 점수 UI, HP UI, XP UI, Level UI 구현
- ExperienceManager 기반 경험치/레벨업 시스템 구현
- PlayerStats를 두고 이동속도/공격력/공속/최대체력/XP획득량 multiplier를 중앙 관리하는 구조

[서버 작업 목표] (VSur_Server 폴더)
1. C# .NET TCP 서버
2. MySQL을 연동하여 데이터 관리 (유저 데이터, 상점, 랭킹 등)

[서버 구조]
- VSurvServer.Server -> 콘솔 앱, .NET 8
- VSurvServer.Core -> 클래스 라이브러리
- VSurvServer.Protocol -> 클래스 라이브러리
- VSurvServer.Infrastructure -> 클래스 라이브러리

[서버 참조 방향]
- Server -> Core, Protocol, Infrastructure
- Core -> Protocol
- Protocol -> 없음
- Infrastructure -> 없음

[패킷 구조]
- 수동 바이너리 헤더 + json Payload 직렬화
(앞 2바이트: 전체 패킷 크기, 다음 1바이트: PacketId, 나머지: Payload)
PacketId -> byte 기반 enum (서버와 Unity 모두 별도 파일로 분리해서 관리)

[네트워크 현황]
1. Unity
- Assets/Scripts/Network/GameServerClient.cs 를 두고 서버 통신 전담으로 사용
- UI가 직접 소켓을 다루지 않고 GameManager -> GameServerClient 흐름으로 가는 방향

2. Server
- ClientSession.cs 로 tcp 통신
