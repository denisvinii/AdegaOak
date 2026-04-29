'use client';

import { useEffect, useState } from 'react';
import api from '@/lib/api';
import { Plus, Users, Shield, User, Edit2, Trash2, UserX, UserCheck } from 'lucide-react';
import Modal from '@/components/Modal';
import type { UsuarioDto, CreateUsuarioRequest, UpdateUsuarioRequest } from '@/types/usuario';

interface FormData {
  nome: string;
  username: string;
  senha: string;
  confirmarSenha: string;
  role: 'admin' | 'funcionario';
  ativo: boolean;
}

export default function UsuariosPage() {
  const [usuarios, setUsuarios] = useState<UsuarioDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UsuarioDto | null>(null);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState<FormData>({
    nome: '',
    username: '',
    senha: '',
    confirmarSenha: '',
    role: 'funcionario' as 'admin' | 'funcionario',
    ativo: true,
  });

  useEffect(() => {
    loadUsuarios();
  }, []);

  const loadUsuarios = async () => {
    try {
      const { data } = await api.get<UsuarioDto[]>('/auth/usuarios');
      setUsuarios(Array.isArray(data) ? data : []);
    } catch (error: any) {
      console.error('Erro ao carregar usuários:', error);
      setUsuarios([]);
    } finally {
      setLoading(false);
    }
  };

  const openCreateModal = () => {
    setEditingUser(null);
    setFormData({
      nome: '',
      username: '',
      senha: '',
      confirmarSenha: '',
      role: 'funcionario' as 'admin' | 'funcionario',
      ativo: true,
    });
    setModalOpen(true);
  };

  const openEditModal = (usuario: UsuarioDto) => {
    setEditingUser(usuario);
    setFormData({
      nome: usuario.nome,
      username: usuario.username,
      senha: '',
      confirmarSenha: '',
      role: usuario.role,
      ativo: usuario.ativo,
    });
    setModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Validação de senha (obrigatória apenas para novo usuário)
    if (!editingUser) {
      if (formData.senha.length < 6) {
        alert('A senha deve ter no mínimo 6 caracteres');
        return;
      }

      if (formData.senha !== formData.confirmarSenha) {
        alert('As senhas não coincidem');
        return;
      }
    } else {
      // Se está editando e preencheu senha, validar
      if (formData.senha && formData.senha.length < 6) {
        alert('A senha deve ter no mínimo 6 caracteres');
        return;
      }

      if (formData.senha && formData.senha !== formData.confirmarSenha) {
        alert('As senhas não coincidem');
        return;
      }
    }

    setSaving(true);

    try {
      if (editingUser) {
        // Atualizar usuário existente
        const updateData: UpdateUsuarioRequest = {
          nome: formData.nome,
          role: formData.role,
          ativo: formData.ativo,
        };

        // Só incluir senha se foi preenchida
        if (formData.senha) {
          updateData.password = formData.senha;
        }

        await api.put(`/auth/usuarios/${editingUser.id}`, updateData);
        alert('Usuário atualizado com sucesso!');
      } else {
        // Criar novo usuário
        const createData: CreateUsuarioRequest = {
          nome: formData.nome,
          username: formData.username,
          password: formData.senha,
          role: formData.role,
        };

        await api.post('/auth/usuarios', createData);
        alert('Usuário cadastrado com sucesso!');
      }

      setModalOpen(false);
      setEditingUser(null);
      loadUsuarios();
    } catch (error: any) {
      console.error('Erro ao salvar usuário:', error);
      alert(error.response?.data?.message || 'Erro ao salvar usuário');
    } finally {
      setSaving(false);
    }
  };

  const handleToggleAtivo = async (usuario: UsuarioDto) => {
    if (!confirm(`Deseja ${usuario.ativo ? 'desativar' : 'ativar'} o usuário ${usuario.nome}?`)) {
      return;
    }

    try {
      await api.put(`/auth/usuarios/${usuario.id}`, {
        ativo: !usuario.ativo,
      });
      alert(`Usuário ${usuario.ativo ? 'desativado' : 'ativado'} com sucesso!`);
      loadUsuarios();
    } catch (error: any) {
      console.error('Erro ao alterar status:', error);
      alert(error.response?.data?.message || 'Erro ao alterar status do usuário');
    }
  };

  const handleDelete = async (usuario: UsuarioDto) => {
    if (!confirm(`Tem certeza que deseja excluir o usuário ${usuario.nome}? Esta ação não pode ser desfeita.`)) {
      return;
    }

    try {
      await api.delete(`/auth/usuarios/${usuario.id}`);
      alert('Usuário excluído com sucesso!');
      loadUsuarios();
    } catch (error: any) {
      console.error('Erro ao excluir usuário:', error);
      alert(error.response?.data?.message || 'Erro ao excluir usuário');
    }
  };

  const admins = (usuarios || []).filter((u) => u?.role === 'admin');
  const funcionarios = (usuarios || []).filter((u) => u?.role === 'funcionario');

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Usuários</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Gerencie usuários e permissões do sistema
          </p>
        </div>
        <button
          onClick={openCreateModal}
          className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-3 rounded-lg flex items-center gap-2 transition"
        >
          <Plus size={20} />
          Novo Usuário
        </button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Total Usuários</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-white mt-2">
                {(usuarios || []).length}
              </p>
            </div>
            <div className="bg-blue-500 p-3 rounded-lg">
              <Users className="text-white" size={24} />
            </div>
          </div>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Administradores</p>
              <p className="text-2xl font-bold text-amber-600 dark:text-amber-400 mt-2">
                {admins.length}
              </p>
            </div>
            <div className="bg-amber-500 p-3 rounded-lg">
              <Shield className="text-white" size={24} />
            </div>
          </div>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-100 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Funcionários</p>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400 mt-2">
                {funcionarios.length}
              </p>
            </div>
            <div className="bg-green-500 p-3 rounded-lg">
              <User className="text-white" size={24} />
            </div>
          </div>
        </div>
      </div>

      {/* Users List */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-900">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Nome
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Usuário
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Função
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Criado em
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Ações
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {(usuarios || []).map((usuario) => (
              <tr key={usuario?.id || Math.random()} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-amber-100 dark:bg-amber-900 rounded-full flex items-center justify-center">
                      <span className="text-amber-600 dark:text-amber-400 font-semibold">
                        {(usuario?.nome || 'U').charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <span className="text-sm font-medium text-gray-900 dark:text-white">
                      {usuario?.nome || 'Usuário'}
                    </span>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                  {usuario?.username || '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      usuario?.role === 'admin'
                        ? 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200'
                        : 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                    }`}
                  >
                    {usuario?.role === 'admin' ? 'Administrador' : 'Funcionário'}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      usuario?.ativo
                        ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                        : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                    }`}
                  >
                    {usuario?.ativo ? 'Ativo' : 'Inativo'}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                  {usuario?.criadoEm ? new Date(usuario.criadoEm).toLocaleDateString('pt-BR') : '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">
                  <div className="flex items-center gap-3">
                    <button
                      onClick={() => openEditModal(usuario)}
                      className="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium inline-flex items-center gap-1"
                      title="Editar usuário"
                    >
                      <Edit2 size={16} />
                      Editar
                    </button>
                    <button
                      onClick={() => handleToggleAtivo(usuario)}
                      className={`${
                        usuario.ativo
                          ? 'text-orange-600 dark:text-orange-400 hover:text-orange-700'
                          : 'text-green-600 dark:text-green-400 hover:text-green-700'
                      } font-medium inline-flex items-center gap-1`}
                      title={usuario.ativo ? 'Desativar' : 'Ativar'}
                    >
                      {usuario.ativo ? <UserX size={16} /> : <UserCheck size={16} />}
                      {usuario.ativo ? 'Desativar' : 'Ativar'}
                    </button>
                    <button
                      onClick={() => handleDelete(usuario)}
                      className="text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 font-medium inline-flex items-center gap-1"
                      title="Excluir usuário"
                    >
                      <Trash2 size={16} />
                      Excluir
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {(usuarios || []).length === 0 && (
          <div className="text-center py-12 text-gray-500 dark:text-gray-400">
            Nenhum usuário cadastrado
          </div>
        )}
      </div>

      {/* Modal Criar/Editar Usuário */}
      <Modal
        isOpen={modalOpen}
        onClose={() => {
          setModalOpen(false);
          setEditingUser(null);
        }}
        title={editingUser ? 'Editar Usuário' : 'Novo Usuário'}
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Nome Completo *
            </label>
            <input
              type="text"
              required
              value={formData.nome}
              onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Ex: João Silva"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Nome de Usuário *
            </label>
            <input
              type="text"
              required
              disabled={!!editingUser}
              value={formData.username}
              onChange={(e) => setFormData({ ...formData, username: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              placeholder="Ex: joao.silva"
            />
            {editingUser && (
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                O nome de usuário não pode ser alterado
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Função *
            </label>
            <select
              required
              value={formData.role}
              onChange={(e) => setFormData({ ...formData, role: e.target.value as 'admin' | 'funcionario' })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option value="funcionario">Funcionário</option>
              <option value="admin">Administrador</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Senha {editingUser ? '(deixe em branco para não alterar)' : '*'}
            </label>
            <input
              type="password"
              required={!editingUser}
              minLength={6}
              value={formData.senha}
              onChange={(e) => setFormData({ ...formData, senha: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder={editingUser ? 'Deixe em branco para manter a senha atual' : 'Mínimo 6 caracteres'}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Confirmar Senha {editingUser ? '' : '*'}
            </label>
            <input
              type="password"
              required={!editingUser && !!formData.senha}
              minLength={6}
              value={formData.confirmarSenha}
              onChange={(e) => setFormData({ ...formData, confirmarSenha: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              placeholder="Digite a senha novamente"
            />
          </div>

          {editingUser && (
            <div>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={formData.ativo}
                  onChange={(e) => setFormData({ ...formData, ativo: e.target.checked })}
                  className="w-4 h-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500"
                />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Usuário ativo
                </span>
              </label>
            </div>
          )}

          {formData.senha && formData.confirmarSenha && formData.senha !== formData.confirmarSenha && (
            <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3">
              <p className="text-sm text-red-800 dark:text-red-300">
                As senhas não coincidem
              </p>
            </div>
          )}

          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={() => {
                setModalOpen(false);
                setEditingUser(null);
              }}
              className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={saving || (formData.senha !== formData.confirmarSenha && formData.confirmarSenha !== '')}
              className="flex-1 px-4 py-2 bg-amber-600 hover:bg-amber-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg transition"
            >
              {saving ? 'Salvando...' : editingUser ? 'Atualizar' : 'Criar'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
