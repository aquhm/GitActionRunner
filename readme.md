# Git Action Runner

![Windows](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)
![.NET 6.0+](https://img.shields.io/badge/.NET-6.0%2B-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green)

Windows 데스크톱 환경에서 GitHub Actions 워크플로우를 관리하는 GUI 애플리케이션입니다.  
개발자 친화적인 인터페이스로 복잡한 CI/CD 파이프라인을 손쉽게 제어할 수 있습니다.

## ✨ 주요 기능

- **GitHub 계정 통합** - Personal Access Token 기반 원클릭 인증으로 안전하고 편리한 GitHub 계정 연동
- **실시간 모니터링** - 워크플로우 실행 상태 실시간 추적 및 시각화, Windows 네이티브 알림 지원
- **브랜치 관리** - 리포지토리별 브랜치 선택 및 워크플로우 실행, 상세한 실행 이력 조회
- **인터페이스 최적화** - 직관적인 UI/UX 설계와 다크 테마 지원으로 사용자 편의성 극대화
- **상태 관리** - 워크플로우 실행 상태(성공/실패/진행 중)에 따른 시각적 피드백 제공
- **보안 저장소** - Windows 데이터 보호 API를 활용한 토큰 안전 저장

## 🛠️ 기술 스택

- **.NET 9.0**: 최신 .NET 프레임워크 기반의 안정적인 실행 환경
- **WPF**: 풍부한 데스크톱 UI 구현을 위한 프레젠테이션 프레임워크
- **MVVM Pattern**: CommunityToolkit.Mvvm을 활용한 클린 아키텍처 구현
- **GitHub API**: Octokit.NET을 통한 효율적인 GitHub 통합
- **의존성 주입**: Microsoft.Extensions.DependencyInjection을 통한 컴포넌트 관리
- **로깅 시스템**: Serilog를 활용한 진단 및 문제 추적

## 📦 의존성

```xml
<!-- UI 및 MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2"/>
<PackageReference Include="ReactiveUI.WPF" Version="20.1.63" />
<!-- 의존성 주입 -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0"/>
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0"/>
<!-- GitHub API -->
<PackageReference Include="Octokit" Version="9.1.0"/>
<!-- 알림 시스템 -->
<PackageReference Include="DesktopNotifications" Version="1.3.1" />
<PackageReference Include="DesktopNotifications.Windows" Version="1.3.1" />
<PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" Version="7.1.3" />
<!-- 로깅 -->
<PackageReference Include="Serilog" Version="3.1.1"/>
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1"/>
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0"/>
```

## 🏗️ 아키텍처

### MVVM 아키텍처 다이어그램

```mermaid
flowchart TB
    subgraph View["View 계층"]
        direction TB
        GHLoginView["GitHubLoginView"]
        RepoListView["RepositoryListView"]
        LoadingInd["LoadingIndicator"]
        MainWin["MainWindow"]
    end
    
    subgraph ViewModel["ViewModel 계층"]
        direction TB
        BaseVM["BaseViewModel"]
        GHLoginVM["GitHubLoginViewModel"]
        RepoListVM["RepositoryListViewModel"]
    end
    
    subgraph Services["서비스 계층"]
        direction TB
        NavService["NavigationService"]
        GHService["GitHubService"]
        AuthService["AuthenticationService"]
        NotifService["NotificationService"]
    end
    
    subgraph Model["Model 계층"]
        direction TB
        Repository["Repository"]
        WorkflowRun["WorkflowRun"]
        SecStorage["SecureStorage"]
    end
    
    %% 관계 정의
    GHLoginView <--> GHLoginVM
    RepoListView <--> RepoListVM
    LoadingInd --> BaseVM
    MainWin --> NavService
    
    GHLoginVM -.-> BaseVM
    RepoListVM -.-> BaseVM
    
    GHLoginVM <--> GHService
    GHLoginVM <--> AuthService
    GHLoginVM <--> NavService
    
    RepoListVM <--> GHService
    RepoListVM <--> AuthService
    RepoListVM <--> NavService
    RepoListVM --> NotifService
    
    GHService <--> AuthService
    AuthService <--> SecStorage
    
    classDef viewClass fill:#42b883,color:white,stroke:#333,stroke-width:1px;
    classDef viewModelClass fill:#2c3e50,color:white,stroke:#333,stroke-width:1px;
    classDef serviceClass fill:#4b69fd,color:white,stroke:#333,stroke-width:1px;
    classDef modelClass fill:#f8d24b,color:black,stroke:#333,stroke-width:1px;
    
    class GHLoginView,RepoListView,LoadingInd,MainWin viewClass;
    class BaseVM,GHLoginVM,RepoListVM viewModelClass;
    class NavService,GHService,AuthService,NotifService serviceClass;
    class Repository,WorkflowRun,SecStorage modelClass;
```

### 인증 프로세스 시퀀스 다이어그램

```mermaid
sequenceDiagram
    participant User as 사용자
    participant Login as GitHubLoginView
    participant LoginVM as GitHubLoginViewModel
    participant GitHubSvc as GitHubService
    participant AuthSvc as AuthenticationService
    participant SecureSt as SecureStorage
    participant NavSvc as NavigationService
    
    User->>Login: 토큰 입력
    Login->>LoginVM: 토큰 입력 및 로그인 명령
    LoginVM->>GitHubSvc: AuthenticateAsync(token)
    GitHubSvc-->>GitHub: API 인증 요청
    GitHub-->>GitHubSvc: 인증 응답
    
    alt 인증 성공
        GitHubSvc->>AuthSvc: SaveAccessTokenAsync(token)
        AuthSvc->>SecureSt: SaveAsync("github_token", token)
        SecureSt-->>SecureSt: 토큰 암호화 및 저장
        GitHubSvc-->>LoginVM: 인증 성공 반환
        LoginVM->>NavSvc: NavigateTo<RepositoryListView>()
        NavSvc->>RepoView: 화면 전환
        RepoView-->>User: 리포지토리 목록 표시
    else 인증 실패
        GitHubSvc-->>LoginVM: 인증 실패 반환
        LoginVM-->>Login: 오류 메시지 표시
        Login-->>User: 인증 실패 알림
    end
```

### 프로젝트 구조

```
GitActionRunner/
├── Core/                 # 비즈니스 로직 레이어
│   ├── Interfaces/      # 서비스 계약 정의
│   │   ├── IAuthenticationService.cs    # 인증 서비스 인터페이스
│   │   ├── IGitHubService.cs           # GitHub API 서비스 인터페이스
│   │   ├── INavigationAware.cs         # 네비게이션 인식 인터페이스
│   │   ├── INavigationService.cs       # 네비게이션 서비스 인터페이스
│   │   └── ISecureStorage.cs           # 보안 저장소 인터페이스
│   ├── Models/          # 도메인 모델
│   │   ├── Repository.cs               # 리포지토리 모델
│   │   └── WorkflowRun.cs              # 워크플로우 실행 모델
│   └── Services/        # 핵심 서비스 구현
│       ├── GitHubService.cs            # GitHub API 통합
│       └── NotificationService.cs      # 알림 서비스
├── ViewModels/          # MVVM 아키텍처
│   ├── BaseViewModel.cs               # 기본 ViewModel 추상 클래스
│   ├── GitHubLoginViewModel.cs        # 로그인 화면 로직
│   └── RepositoryListViewModel.cs      # 저장소 목록 로직
├── Views/               # UI 레이어
│   ├── GitHubLoginView.xaml           # 로그인 화면
│   └── RepositoryListView.xaml        # 저장소 목록 화면
├── Controls/            # 커스텀 컨트롤
│   ├── LoadingIndicator.xaml          # 로딩 인디케이터
│   └── PlaceholderTextBox.cs          # 플레이스홀더 텍스트박스
├── Converters/          # 값 변환기
└── Services/            # 인프라 서비스
    └── NavigationService.cs           # 네비게이션 서비스
```

## 주요 특징 및 설계 원칙

### MVVM 패턴 구현

애플리케이션은 CommunityToolkit.Mvvm 라이브러리를 활용하여 MVVM(Model-View-ViewModel) 패턴을 구현하였습니다:

- **Model**: Core/Models 디렉토리의 도메인 모델 클래스들
- **View**: 사용자 인터페이스를 담당하는 XAML 기반 Views
- **ViewModel**: 비즈니스 로직과 UI 상태를 관리하는 ViewModels

### 의존성 주입

- Microsoft.Extensions.DependencyInjection을 사용하여 모든 컴포넌트 간의 느슨한 결합 구현
- 인터페이스 기반 설계로 유닛 테스트 용이성 향상 및 확장성 제공
- 애플리케이션 시작 시 App.xaml.cs에서 서비스 등록 및 구성

### 로깅 시스템

- Serilog를 활용한 구조화된 로깅
- 일별 로그 파일 생성 및 관리
- 다양한 로그 레벨 지원 (Debug, Information, Warning, Error, Fatal)

## 📱 스크린샷

### 로그인 화면
![login.png](./doc/images/login.png)
GitHub Personal Access Token을 통한 안전한 인증

### 리포지토리 목록
![repository.png](./doc/images/repository.png)
사용자 리포지토리 목록 및 워크플로우 상태 확인

### 워크플로우 실행
![repository1.png](./doc/images/repository1.png)
브랜치 선택 및 워크플로우 실행 관리

### 실행 이력
![repository2.png](./doc/images/repository2.png)
워크플로우 실행 이력 및 상태 모니터링

### 알림 시스템
![notification.png](./doc/images/notification.png)
Windows 네이티브 알림을 통한 실시간 상태 업데이트

## 🚀 시작하기

1. GitHub Personal Access Token 발급 (필요 권한: `repo`, `workflow`)
2. 애플리케이션 실행 및 토큰 입력
3. 리포지토리 선택 후 워크플로우 관리 시작

## 🧪 개발자 안내

### 개발 환경 설정
1. Visual Studio 2022 이상 설치
2. .NET 9.0 SDK 설치
3. 프로젝트 클론 및 NuGet 패키지 복원

### 빌드 및 실행
```bash
dotnet restore
dotnet build
dotnet run
```

## 📝 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다.
