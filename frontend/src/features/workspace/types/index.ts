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
  id: string;
  fullName: string;
  email?: string;
  avatarUrl?: string;
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

