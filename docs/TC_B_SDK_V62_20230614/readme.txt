针对门禁设备员工的门禁有效期,增加的扩展接口:

/****************************************************************
* 获取员工资料, 包含员工门禁有效期，及排班时间
* 信息返回类型:CCHEX_RET_DLEMPLOYEE_SCHEDULING_INFO_TYPE
* 信息解析结构:CCHEX_RET_DLEMPLOYEE_SCHEDULING_INFO_STRU
****************************************************************/
CChex_ModifyPersonInfoEx(
API_EXTERN int CChex_GetPersonInfoEx(void *CchexHandle, int DevIdx, CCHEX_GET_EMPLOYEE_SCH_INFO_STRU *employee);
/****************************************************************
* 修改员工资料, 包含员工门禁有效期，及排班时间
* 信息返回类型:CCHEX_RET_ULEMPLOYEE2W2_INFO_TYPE
* 信息解析结构:CCHEX_RET_COMMON_WITH_EMPLOYEE_ID       CCHEX_RET_COMMON__WITH_EMPLOYEE_VER_4_NEWID
*************************************************************************************************/
API_EXTERN int CChex_ModifyPersonInfoEx(void *CchexHandle, int DevIdx, CCHEX_RET_DLEMPLOYEE_SCHEDULING_INFO_STRU *employee, unsigned char employeeNum);