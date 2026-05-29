export interface BoardSummaryDto {
  id: string;
  workspaceId: string;
  name: string;
  createdByName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateBoardRequest {
  workspaceId: string;
  name: string;
}
