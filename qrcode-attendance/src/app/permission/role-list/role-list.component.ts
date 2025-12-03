import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzMessageService } from 'ng-zorro-antd/message';
import { catchError, of } from 'rxjs';
import { PermissionService } from '../../core/services/permission.service';

@Component({
  selector: 'app-role-list',
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss']
})
export class RoleListComponent implements OnInit {
  list: any[] = [];
  loading = false;
  pageIndex = 1;
  pageSize = 10;
  total = 0;
  keyword = '';

  isGroupsVisible = false;
  groupsFlat: any[] = [];
  groupLoading = false;
  savingMap: Record<string, boolean> = {};
  pendingChanges: Record<string, boolean> = {};
  // highlightedFunctionId: any = null;
  currentRoleForGroups: any = null;
  expandedGroups: Set<any> = new Set<any>();
  allowedTopLevelCodes: Set<string> = new Set(['QLHP','QLND','NKHT','LGD','QLDD','BCLH','LH']);

  editVisible = false;
  editForm!: FormGroup;
  editingRole: any = null;

  constructor(private perm: PermissionService, private fb: FormBuilder, private msg: NzMessageService, private modal: NzModalService) {
  this.editForm = this.fb.group({ CodeQuyen: ['', Validators.required], TenQuyen: ['', Validators.required], MoTa: [''], TrangThai: [true] });
  }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    const params: any = { Page: this.pageIndex, PageSize: this.pageSize };
    const kw = (this.keyword || '').trim();
    if (kw) params.TenQuyen = kw;

    this.perm.getRoles(params).subscribe({ next: (res: any) => {
      const raw = res?.data?.items || res?.data || res || [];
      const coerceBool = (v: any, defaultValue = true) => {
        if (v === undefined || v === null) return defaultValue;
        if (typeof v === 'boolean') return v;
        if (typeof v === 'number') return v === 1;
        const s = String(v).toLowerCase();
        if (s === 'true' || s === '1') return true;
        if (s === 'false' || s === '0') return false;
        return defaultValue;
      };

      this.list = (raw || []).map((it: any) => {
        const p = it?.phanQuyen || it?.phan_quyen || it || {};
        return {
          MaQuyen: p?.maQuyen || p?.MaQuyen || p?.ma || undefined,
          maQuyen: p?.maQuyen || p?.MaQuyen || p?.ma || undefined,
          CodeQuyen: p?.codeQuyen || p?.CodeQuyen || p?.code || undefined,
          codeQuyen: p?.codeQuyen || p?.CodeQuyen || p?.code || undefined,
          TenQuyen: p?.tenQuyen || p?.TenQuyen || p?.ten || undefined,
          tenQuyen: p?.tenQuyen || p?.TenQuyen || p?.ten || undefined,
          MoTa: p?.moTa || p?.MoTa || p?.mo_ta || undefined,
          moTa: p?.moTa || p?.MoTa || p?.mo_ta || undefined,
          TrangThai: coerceBool(p?.trangThai || p?.TrangThai),
          trangThai: coerceBool(p?.trangThai || p?.TrangThai)
        };
      });

      const totalRaw = res?.data?.totalRecords ?? res?.data?.total;
      this.total = Number(totalRaw ?? this.list.length) || this.list.length;
      this.loading = false;
    }, error: (err: any) => { this.loading = false; this.msg.error('Không thể tải danh sách phân quyền'); } });
  }

  onPageChange(p: number) { this.pageIndex = p; this.load(); }

  onSearch() { this.pageIndex = 1; this.load(); }
  showCreate() { this.editingRole = null; this.editForm.reset({ TrangThai: true }); this.editVisible = true; }

  showEdit(item: any) { this.editingRole = item; this.editForm.patchValue(item); this.editVisible = true; }

  refreshList(): void {
    this.keyword = '';
    this.pageIndex = 1;
    this.load();
  }

  save() {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    const value = this.editForm.value;
    if (this.editingRole) {
      const payload = { MaQuyen: this.editingRole.MaQuyen || this.editingRole.maQuyen, ...value };
      this.perm.updateRole(payload).subscribe({ next: (res) => { this.msg.success('Cập nhật thành công'); this.editVisible = false; this.load(); }, error: () => { this.msg.error('Lỗi cập nhật'); } });
    } else {
      this.perm.createRole(value).subscribe({ next: () => { this.msg.success('Tạo thành công'); this.editVisible = false; this.load(); }, error: () => { this.msg.error('Lỗi tạo phân quyền'); } });
    }
  }

  delete(item: any) {
    const ma = item.MaQuyen || item.maQuyen;
    this.perm.deleteRole(ma).subscribe({ next: () => { this.msg.success('Xóa thành công'); this.load(); }, error: () => { this.msg.error('Lỗi xóa phân quyền'); } });
  }

  confirmDelete(item: any): void {
    const title = `Bạn có chắc muốn xóa quyền '${item?.tenQuyen || item?.codeQuyen || ''}'?`;
    this.modal.confirm({ nzTitle: title, nzOkText: 'Xóa', nzOkDanger: true, nzOnOk: () => this.delete(item), nzCancelText: 'Hủy' });
  }

  openGroupsForRole(_role: any): void {
    // this.highlightedFunctionId = null;
    this.isGroupsVisible = true;
    this.currentRoleForGroups = _role;
    this.expandedGroups = new Set<any>();
    this.loadGroupsForRole(_role);
  }

  // loadGroupsHierarchy(): void {
  //   this.groupLoading = true;
  //   this.perm.getFunctions().pipe(catchError(() => of(null))).subscribe({ next: (res: any) => {
  //     const raw = Array.isArray(res) ? res : (res?.data?.items || res?.data || res || []);
  //     const extracted = (raw || []).map((it: any) => it?.chucNang || it || {});
  //     const normalized = (extracted || []).map((c: any) => ({
  //       maChucNang: c?.maChucNang ?? c?.MaChucNang ?? c?.id,
  //       codeChucNang: c?.codeChucNang ?? c?.CodeChucNang ?? c?.code,
  //       tenChucNang: c?.tenChucNang ?? c?.TenChucNang ?? c?.ten,
  //       trangThai: c?.trangThai ?? c?.TrangThai ?? true,
  //       parentIdRaw: c?.parentChucNangId ?? c?.ParentChucNangId ?? c?.parentId ?? c?.ParentId ?? null
  //     }));

  //     normalized.forEach((n: any) => { if (n.parentIdRaw === 'null' || n.parentIdRaw === '') n.parentId = null; else n.parentId = n.parentIdRaw; });
  //     const parents = normalized.filter((n: any) => n.parentId === null || n.parentId === undefined);
  //     const children = normalized.filter((n: any) => n.parentId !== null && n.parentId !== undefined);
  //     const flat: any[] = [];
  //     parents.forEach((p: any) => {
  //       flat.push({ ...p, level: 1 });
  //       const kids = children.filter((ch: any) => String(ch.parentId) === String(p.maChucNang));
  //       kids.forEach((k: any) => flat.push({ ...k, level: 2 }));
  //     });
  //     this.groupsFlat = flat;
  //     this.groupLoading = false;
  //   }, error: () => { this.groupLoading = false; this.msg.error('Lấy nhóm chức năng thất bại'); } });
  // }

  loadGroupsForRole(role: any): void {
    this.groupLoading = true;
    const ma = role?.MaQuyen || role?.maQuyen || role?.MaQuyen;
    this.perm.getRoleFunctions({ maQuyen: ma }).pipe(catchError(() => of(null))).subscribe({ next: (res: any) => {
      const raw = res?.data?.items || res?.data || res || [];
      const extracted = (raw || []).map((it: any) => ({
        maChucNang: it?.chucNang?.maChucNang ?? it?.chucNang?.MaChucNang ?? it?.maChucNang,
        codeChucNang: it?.chucNang?.codeChucNang ?? it?.chucNang?.CodeChucNang,
        tenChucNang: it?.chucNang?.tenChucNang ?? it?.chucNang?.TenChucNang,
        parentIdRaw: it?.chucNang?.parentChucNangId ?? it?.chucNang?.ParentChucNangId ?? null,
        trangThai: (it?.nhomChucNang && (it.nhomChucNang.trangThai === true || String(it.nhomChucNang.trangThai).toLowerCase() === 'true')) ? true : false
      }));

      extracted.forEach((n: any) => { if (n.parentIdRaw === 'null' || n.parentIdRaw === '') n.parentId = null; else n.parentId = n.parentIdRaw; });
      const parents = extracted.filter((n: any) => n.parentId === null || n.parentId === undefined);
      const children = extracted.filter((n: any) => n.parentId !== null && n.parentId !== undefined);
      const flat: any[] = [];
      parents.forEach((p: any) => {
        flat.push({ ...p, level: 1 });
        const kids = children.filter((ch: any) => String(ch.parentId) === String(p.maChucNang));
        kids.forEach((k: any) => flat.push({ ...k, level: 2 }));
      });
      this.groupsFlat = flat;
      this.expandedGroups = new Set<any>();
      this.savingMap = {};
      this.pendingChanges = {};
      this.groupsFlat.forEach((g: any) => { this.savingMap[g.maChucNang] = false; });
      this.groupLoading = false;
    }, error: () => { this.groupLoading = false; this.msg.error('Lấy nhóm chức năng cho quyền thất bại'); } });
  }

  onToggleItem(it: any, newVal?: any): void {
    if (!this.currentRoleForGroups) { this.msg.error('Không có quyền hiện tại'); return; }
    const resolvedNewVal = (newVal === undefined) ? !!it.trangThai : !!newVal;
    try { it.trangThai = resolvedNewVal; } catch (e) {}
    this.pendingChanges = this.pendingChanges || {};
    this.pendingChanges[it.maChucNang] = resolvedNewVal;
    if (it && it.level === 1) {
      const parentId = it.maChucNang;
      (this.groupsFlat || []).forEach((g: any) => {
        if (g.level === 2 && String(g.parentId) === String(parentId)) {
          try { g.trangThai = resolvedNewVal; } catch (e) {}
          this.pendingChanges[g.maChucNang] = resolvedNewVal;
        }
      });
    }
    if (it && it.level === 2) {
      const parentId = it.parentId;
      if (parentId !== null && parentId !== undefined) {
        const kids = (this.groupsFlat || []).filter((g: any) => g.level === 2 && String(g.parentId) === String(parentId));
        const allChecked = kids.length ? kids.every((k: any) => !!k.trangThai) : false;
        const parent = (this.groupsFlat || []).find((g: any) => g.level === 1 && String(g.maChucNang) === String(parentId));
        if (parent) {
          try { parent.trangThai = allChecked; } catch (e) {}
          this.pendingChanges[parent.maChucNang] = allChecked;
        }
      }
    }
  }

  applyGroupChanges(): void {
    if (!this.currentRoleForGroups) { this.msg.error('Không có quyền hiện tại'); return; }
    const maQuyen = this.currentRoleForGroups?.MaQuyen || this.currentRoleForGroups?.maQuyen;
    const keys = Object.keys(this.pendingChanges || {}).filter(k => this.pendingChanges[k] !== undefined);
    if (!keys.length) { this.isGroupsVisible = false; return; }
    let remaining = keys.length;
    keys.forEach((ma) => {
      const newVal = !!this.pendingChanges[ma];
      const payload: any = { TrangThai: newVal, FromMaQuyen: maQuyen, FromMaChucNang: ma, _onlyFrom: true };
      this.savingMap[ma] = true;
      this.perm.updateRoleFunctionGroup(payload).subscribe({ next: () => {
        this.savingMap[ma] = false;
        remaining--; if (remaining === 0) {
          this.msg.success('Cập nhật trạng thái nhóm chức năng hoàn tất');
          this.loadGroupsForRole(this.currentRoleForGroups);
          try { window.dispatchEvent(new CustomEvent('permissions:changed')); } catch (e) {}
          this.isGroupsVisible = false;
        }
      }, error: () => {
        this.savingMap[ma] = false;
        remaining--; if (remaining === 0) {
          this.msg.error('Cập nhật một số mục thất bại');
          this.loadGroupsForRole(this.currentRoleForGroups);
          try { window.dispatchEvent(new CustomEvent('permissions:changed')); } catch (e) {}
          this.isGroupsVisible = false;
        }
      } });
    });
    this.pendingChanges = {};
  }

  toggleGroup(maChucNang: any): void {
    if (this.expandedGroups.has(maChucNang)) this.expandedGroups.delete(maChucNang);
    else this.expandedGroups.add(maChucNang);
  }

  isExpanded(maChucNang: any): boolean {
    return this.expandedGroups.has(maChucNang);
  }

  // hasChildren(maChucNang: any): boolean {
  //   if (!this.groupsFlat || !this.groupsFlat.length) return false;
  //   return this.groupsFlat.some(g => g.level === 2 && String(g.parentId) === String(maChucNang));
  // }

  hasToggle(it: any): boolean {
    if (!it) return false;
    if (it.level !== 1) return false;
    const code = String(it.codeChucNang || it.CodeChucNang || it.code || '').toUpperCase();
    return this.allowedTopLevelCodes.has(code);
  }

  isParentExpanded(parentId: any): boolean {
    if (parentId === null || parentId === undefined) return true;
    return this.expandedGroups.has(parentId);
  }

  cancelGroups(): void {
    if (this.currentRoleForGroups) this.loadGroupsForRole(this.currentRoleForGroups);
    this.isGroupsVisible = false;
  }
}
