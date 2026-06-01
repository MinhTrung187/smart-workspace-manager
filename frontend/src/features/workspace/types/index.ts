export interface ColumnDto {
  id: string;
  boardId: string;
  name: string;
  position: number;
}

export interface BoardDto {
  id: string;
  workspaceId: string;
  name: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
  columns: ColumnDto[];
}
export interface WorkspaceMemberDto {
  userId: string;
  fullName: string;
  email: string;
  avatarUrl: string | null;
  role: string;
  joinedAt: string;
  id?: string; 
}


export interface WorkspaceDetailDto {
  id: string;
  name: string;
  description: string;
  ownerName: string;
  createdAt: string;
  updatedAt: string;
  boards: BoardDto[];
  members?: WorkspaceMemberDto[];
  memberCount?: number;
}

export interface WorkspaceSummaryDto {
  id: string;
  name: string;
  description: string;
  ownerName: string;
  createdAt: string;
}

export interface CreateWorkspaceRequest {
  name: string;
  description?: string;
}
export interface GetAllWorkspacesResponse {
  totalWorkspacesCount: number;
  workspaces: WorkspaceSummaryDto[];
}

