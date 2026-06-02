import { useState, useRef, useEffect } from 'react';
import type { FormEvent } from 'react';
import { MessageSquare, X, Send, Loader2, MessageCircle } from 'lucide-react';
import { useChannelMessagesQuery, useSendMessageMutation } from '../hooks/useChat';

interface WorkspaceChatPanelProps {
  channelId: string;
  workspaceName: string;
}

export default function WorkspaceChatPanel({ channelId, workspaceName }: WorkspaceChatPanelProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [typedMessage, setTypedMessage] = useState('');
  const [unreadCount, setUnreadCount] = useState(0);
  const [lastMessageId, setLastMessageId] = useState<string | null>(null);

  const messagesEndRef = useRef<HTMLDivElement | null>(null);
  const messageContainerRef = useRef<HTMLDivElement | null>(null);

  const { data: messages, isLoading, isError, refetch } = useChannelMessagesQuery(channelId);
  const sendMessageMutation = useSendMessageMutation(channelId);

  const scrollToBottom = (behavior: 'smooth' | 'auto' = 'smooth') => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior });
    }
  };

  useEffect(() => {
    if (messages && messages.length > 0) {
      const latestMessage = messages[messages.length - 1];
      
      if (!isOpen) {
        if (lastMessageId && latestMessage.id !== lastMessageId && !latestMessage.sentByMe) {
          setUnreadCount((prev) => prev + 1);
        }
      } else {
        setUnreadCount(0);
      }
      setLastMessageId(latestMessage.id);
    }
  }, [messages, isOpen]);

  useEffect(() => {
    if (isOpen) {
      setUnreadCount(0);
      const timer = setTimeout(() => {
        scrollToBottom('auto');
      }, 80);
      return () => clearTimeout(timer);
    }
  }, [isOpen, messages?.length]);

  const handleSend = (e: FormEvent) => {
    e.preventDefault();
    const cleanContent = typedMessage.trim();
    if (!cleanContent || sendMessageMutation.isPending) return;

    sendMessageMutation.mutate(cleanContent, {
      onSuccess: () => {
        setTypedMessage('');
        // Smooth scroll to bottom right after sending
        setTimeout(() => {
          scrollToBottom('smooth');
        }, 100);
      },
    });
  };

  const formatMessageTime = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true });
    } catch {
      return '';
    }
  };

  return (
    <>
      {/* Floating Action Button (FAB) at Bottom-Right */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="fixed bottom-6 right-6 z-40 h-14 w-14 rounded-full bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white flex items-center justify-center shadow-xl hover:shadow-2xl transition-all cursor-pointer group hover:scale-105"
        aria-label="Toggle Workspace Chat"
        id="workspace-chat-fab"
      >
        {isOpen ? (
          <X className="w-6 h-6 transition-transform group-hover:rotate-90 duration-200" />
        ) : (
          <MessageSquare className="w-6 h-6 animate-pulse" />
        )}

        {/* Unread Message Notification Dot Badge */}
        {!isOpen && unreadCount > 0 && (
          <span className="absolute -top-1.5 -right-1.5 flex h-6 w-6 items-center justify-center rounded-full bg-rose-500 text-[10px] font-extrabold text-white animate-bounce border-2 border-white shadow-md">
            {unreadCount}
          </span>
        )}
      </button>

      {/* Floating Chat Panel Sidebar */}
      {isOpen && (
        <div
          className="fixed top-4 right-4 bottom-24 md:bottom-4 h-[calc(100vh-2rem)] w-80 md:w-96 bg-white border border-slate-200 shadow-2xl rounded-2xl z-40 flex flex-col overflow-hidden animate-in slide-in-from-right-8 duration-300"
          id="workspace-chat-sidebar"
        >
          {/* Panel Header */}
          <div className="p-4 border-b border-slate-100 bg-slate-50/50 flex justify-between items-center shrink-0">
            <div className="flex items-center gap-2.5 min-w-0">
              <div className="p-2 bg-indigo-50 text-indigo-600 rounded-lg">
                <MessageCircle className="w-5 h-5 animate-pulse" />
              </div>
              <div className="min-w-0">
                <h3 className="text-sm font-bold text-slate-900 truncate pr-2">
                  {workspaceName} Chat
                </h3>
                <span className="text-[10px] uppercase font-bold tracking-wider text-emerald-600 flex items-center gap-1.5 mt-0.5 leading-none">
                  <span className="h-1.5 w-1.5 rounded-full bg-emerald-500 inline-block animate-ping" />
                  Live Channel
                </span>
              </div>
            </div>
            <button
              onClick={() => setIsOpen(false)}
              className="p-1 text-slate-400 hover:text-slate-600 hover:bg-slate-100/80 rounded-md transition-colors"
              aria-label="Close chat panel"
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          {/* Messages List Area */}
          <div
            ref={messageContainerRef}
            className="flex-1 overflow-y-auto p-4 space-y-4 bg-slate-50/30"
          >
            {isLoading ? (
              <div className="flex flex-col items-center justify-center h-full gap-2">
                <Loader2 className="w-6 h-6 text-indigo-600 animate-spin" />
                <span className="text-[11px] text-slate-400 font-semibold">Conversations loading...</span>
              </div>
            ) : isError ? (
              <div className="flex flex-col items-center justify-center h-full text-center p-4">
                <span className="text-xs text-red-500 font-semibold mb-2">Failed to load chat history.</span>
                <button
                  onClick={() => refetch()}
                  className="text-xs font-bold text-indigo-600 hover:underline"
                >
                  Reload Chat
                </button>
              </div>
            ) : messages && messages.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-center max-w-55 mx-auto space-y-2">
                <div className="p-3 bg-indigo-50 text-indigo-400 rounded-full">
                  <MessageSquare className="w-6 h-6" />
                </div>
                <h4 className="text-xs font-extrabold text-slate-700">No Messages Yet</h4>
                <p className="text-[10px] text-slate-400 leading-relaxed">
                  Start the topic and ask colleagues your questions!
                </p>
              </div>
            ) : (
              messages?.map((msg, index) => {
                const showSenderName = !msg.sentByMe && (index === 0 || messages[index - 1].userId !== msg.userId);

                return (
                  <div
                    key={msg.id}
                    className={`flex flex-col ${msg.sentByMe ? 'items-end' : 'items-start'} max-w-full`}
                  >
                    {/* Unique Sender Name */}
                    {showSenderName && (
                      <span className="text-[10px] font-bold text-slate-500 ml-1.5 mb-1">
                        {msg.userName || 'Anonymous'}
                      </span>
                    )}

                    {/* Bubble Layout wrapper */}
                    <div className="flex items-end gap-1.5 max-w-[85%] group">
                      <div
                        className={`px-3.5 py-2 text-xs leading-relaxed shadow-sm wrap-break-word overflow-hidden ${
                          msg.sentByMe
                            ? 'bg-indigo-600 text-white rounded-l-2xl rounded-tr-2xl'
                            : 'bg-white border border-slate-200/80 text-slate-800 rounded-r-2xl rounded-tl-2xl'
                        }`}
                      >
                        <p className="whitespace-pre-wrap">{msg.content}</p>
                      </div>

                      {/* Micro Timestamp indicator */}
                      <span className="text-[8px] text-slate-400/90 font-medium shrink-0 leading-none hidden group-hover:inline-block">
                        {formatMessageTime(msg.createdAt)}
                      </span>
                    </div>
                  </div>
                );
              })
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Chat Form Footer input */}
          <form
            onSubmit={handleSend}
            className="p-3.5 border-t border-slate-100 bg-white flex items-center gap-2 shrink-0"
          >
            <input
              type="text"
              placeholder="Write a message..."
              value={typedMessage}
              onChange={(e) => setTypedMessage(e.target.value)}
              disabled={sendMessageMutation.isPending || isLoading}
              className="flex-1 bg-slate-50 border border-slate-200 focus:bg-white rounded-xl px-3.5 py-2 text-xs text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500/80 transition-all disabled:opacity-60"
            />
            <button
              type="submit"
              disabled={!typedMessage.trim() || sendMessageMutation.isPending || isLoading}
              className="p-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 disabled:bg-slate-150 disabled:text-slate-400 text-white shadow-sm transition-colors active:scale-95 cursor-pointer disabled:cursor-not-allowed shrink-0"
              aria-label="Send message"
            >
              {sendMessageMutation.isPending ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Send className="w-4 h-4" />
              )}
            </button>
          </form>
        </div>
      )}
    </>
  );
}
