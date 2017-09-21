using System;
using System.Collections;
using System.Data;
using System.Text;
//using Oracle.DataAccess.Client;
using Eport.Pub.Log;
using Eport.EMS3.COMM;
using Eport.Pub.Data;
namespace Eport.EMS3
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class Ems3SvrBuildMsgData
    {
        private const string N13_5Format = "FM9999999999990.00000";//Z(12)9.9(5)
        private const string N12_5Format = "FMS000000000000.00000";//+9(12).9(5)
        private const string N9_9Format = "FM999999990.000000000";//Z(8)9.9(9)

        public Ems3SvrBuildMsgData()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static int addSignerInfo(
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            string strICardID,
                                                                            ref string strErrMsg)
        {


            string strTabName = string.Empty;
            string strStatus = string.Empty;
            string strSql = string.Empty;

            switch (strMsgType)
            {
                case "EMS211":
                case "EMS221":
                    strTabName = "PRE_EMS3_TR_HEAD";
                    break;
                case "EMS212":
                case "EMS222":
                    strTabName = "PRE_EMS3_HEAD";
                    break;
                case "EMS213":
                case "EMS223":
                    strTabName = "PRE_EMS3_CUS_HEAD";
                    break;
                case "EMS214":
                case "EMS224":
                    strTabName = "PRE_EMS3_FAS_HEAD";
                    break;
                case "EMS231":
                case "EMS231P":
                    strTabName = "PRE_EMS3_DCR_HEAD";
                    break;
                case "EMS230":
                    strTabName = "PRE_EMS3_COL_HEAD";
                    break;
                //add by zhaoag 2011-11-07
                //出口加工区系统企业物料备案报文
                case "EPZ911"://企业物料备案
                    strTabName = "PRE_EMS3_HEAD";
                    break;
                default:
                    strErrMsg = "报文类型错误！";
                    return -1;
            }

            if (strTabName.Equals("PRE_EMS3_DCR_HEAD"))
            {
                strSql = "UPDATE " + strTabName;
                strSql += " SET ICCARD_ID = {ICCARD_ID},";
                strSql += " DCR_DATE = GETDATE() ";
                strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                strSql += " AND TRADE_CODE = {TRADE_CODE}";
            }
            else if (strTabName.Equals("PRE_EMS3_COL_HEAD"))
            {
                strSql = "UPDATE " + strTabName;
                strSql += " SET ICCARD_ID = {ICCARD_ID},";
                strSql += " INPUT_DATE = GETDATE() ";
                strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                strSql += " AND TRADE_CODE = {TRADE_CODE}";
            }
            else if (strTabName.Equals("PRE_EMS3_FAS_HEAD"))
            {
                strSql = "UPDATE " + strTabName;
                strSql += " SET ID_CARD = {ICCARD_ID},";
                strSql += " DECLARE_DATE = GETDATE() ";
                strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                strSql += " AND TRADE_CODE = {TRADE_CODE}";
            }
            else
            {
                strSql = "UPDATE " + strTabName;
                strSql += " SET ICCARD_ID = {ICCARD_ID},";
                strSql += " DECLARE_DATE = GETDATE() ";
                strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                strSql += " AND TRADE_CODE = {TRADE_CODE}";
            }

            Hashtable HashData = new Hashtable();
            HashData.Add("COP_EMS_NO", strCopEmsNo);
            HashData.Add("TRADE_CODE", strTradeCode);
            HashData.Add("ICCARD_ID", strICardID);

            int bErrCode = Ems3Data.exeSqlByOleDbCmd(strSql, HashData);
            if (bErrCode != 0)
                strErrMsg = "更新申报时间出错！";
            return bErrCode;
        }
        //取HOSTID
        /// <summary>
        /// haodabing 添加的说明：
        /// Ems3Type ＝= "P"，表示是北京保税仓，在配置时，是使用仓库编号充当trade_code。
        ///Ems3Type ！= "P"，表示非北京保税仓，在配置时，若一家企业一个host_id的，trade_code字段填10位有效字符
        /// 若多家企业使用一个host_id，trade_code字段填入这些企业相同的前5位字符，后5位用×表示。这样只要前五位是这个的，host_id都相同
        /// </summary>
        /// <param name="Ems3Type"></param>
        /// <param name="strCopEmsNo"></param>
        /// <param name="strTradeCode"></param>
        /// <param name="strHostId"></param>
        /// <param name="strMastCust"></param>
        /// <param name="strErrMsg"></param>
        /// <returns></returns>
        public static int getHostId(string Ems3Type, string strCopEmsNo, string strTradeCode, ref string strHostId, string strMastCust, ref string strErrMsg)
        {
            strHostId = string.Empty;
            Hashtable QryExp = new Hashtable();
            /*string strSql="SELECT SUBSTR(HOST_ID,-6,6) FROM EMS3_COP_HOST_REG"; by haodabing 20090115*/
            string strSql = "SELECT SUBSTR(HOST_ID,-6,6),MASTER_CUSTOMS FROM EMS3_COP_HOST_REG"; //by haodabing 20090115
            LogMgr.WriteInfo(Ems3Dict.strEms,"tradecode:" + strTradeCode);
            LogMgr.WriteInfo(Ems3Dict.strEms, "ems3type:" + Ems3Type);

            if (Ems3Type != "P")
            {
                strSql += " WHERE TRADE_CODE = {TRADE_CODE}";
                strSql += " OR TRADE_CODE = {TRADE_CODE1}";
                strSql += " ORDER BY TRADE_CODE DESC";

                QryExp.Add("TRADE_CODE", strTradeCode);
                QryExp.Add("TRADE_CODE1", strTradeCode.Substring(0, 5) + "*****");
            }
            else
            {
                strSql += " WHERE TRADE_CODE = {TRADE_CODE2}";

                QryExp.Add("TRADE_CODE2", strCopEmsNo.Substring(0, 4));
            }
            LogMgr.WriteWarning(Ems3Dict.strEms, "sql:" + strSql);

            DataTable tmpTable;
            int bErrCode = Ems3Data.readTableByAdapter(strSql, QryExp, out tmpTable);

            if (bErrCode == -1)
                strErrMsg = "查询企业HOSTID失败！";
            else
            {
                if (tmpTable == null)
                {
                    bErrCode = -1;
                    strErrMsg = "查询不到企业对应HOSTID！";
                }
                else
                {
                    //	strHostId=tmpTable.Rows[0][0].ToString();  //注释by haodabing 20090116 
                    //***************************haodabing 添加
                    if (tmpTable.Rows.Count > 1)
                    {
                        //如果该企业配置了多个host_id，则按关区号来查询
                        DataRow[] selRows = tmpTable.Select("MASTER_CUSTOMS = '" + strMastCust + "'");
                        if (selRows.Length == 0)
                        {
                            LogMgr.WriteInfo(Ems3Dict.strEms, "该企业配置了多个host_id，但是此关区没有配,关区代码：" + strMastCust);
                            bErrCode = -1;
                            strErrMsg = "该企业配置了多个host_id，但是此关区没有配,关区代码：" + strMastCust;
                        }
                        else
                        {
                            strHostId = selRows[0][0].ToString();
                            LogMgr.WriteInfo(Ems3Dict.strEms, "该企业配置了多个host_id，取得的host_id：" + strHostId);
                        }

                    }
                    else
                    {
                        strHostId = tmpTable.Rows[0][0].ToString();
                    }
                    //*******************************************
                }
            }
            return bErrCode;
        }
        //更新状态表
        public static int updateStat(string strMsgType,//报文类型
            string strTradeCode,//企业十位编码
            string strCopEmsNo,//企业内部编号
            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
            IDbTransaction oraTrans,
            ref string strErrMsg)
        {//返回错误信息

            int bErrCode = 0;
            string strSql = string.Empty;
            Hashtable HashData = new Hashtable();
            HashData.Add("COP_EMS_NO", strCopEmsNo);
            HashData.Add("TRADE_CODE", strTradeCode);

            switch (strMsgType)
            {
                case "EMS211"://经营范围备案
                case "EMS221"://经营范围变更
                    strSql = "UPDATE EMS3_TR_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    break;
                case "EMS212"://归并关系备案(归并前、后)EMS222-归并后
                case "EMS222"://归并关系变更(归并前、后)EMS225-归并后
                    strSql = "UPDATE EMS3_MR_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    break;
                case "EMS213"://电子帐册备案
                case "EMS223"://电子帐册变更
                    strSql = "UPDATE EMS3_MESS_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    break;
                case "EMS214"://电子帐册分册备案
                case "EMS224"://电子帐册分册变更
                    strSql = "UPDATE EMS3_FAS_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    break;
                case "EMS231"://报核(正式报核)
                case "EMS231P"://报核(预报核)
                    strSql = "UPDATE EMS3_DCR_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    strSql += " AND DCR_TIMES = {OTHER_PARA}";

                    HashData.Add("OTHER_PARA", strOtherPara);
                    break;
                case "EMS230"://中期核查
                    strSql = "UPDATE EMS3_COL_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    strSql += " AND BEGIN_DATE = {OTHER_PARA}";

                    HashData.Add("OTHER_PARA", strOtherPara);
                    break;
                //add by zhaoag 2011-11-08
                //出口加工区系统企业物料备案报文
                case "EPZ911"://企业物料备案
                    strSql = "UPDATE EMS3_MR_STAT";
                    strSql += " SET OPER_STATUS='D'";
                    strSql += " WHERE COP_EMS_NO = {COP_EMS_NO}";
                    strSql += " AND TRADE_CODE = {TRADE_CODE}";
                    break;
                default:
                    strErrMsg = "报文类型错误!";
                    return -1;
            }
            LogMgr.WriteWarning(Ems3Dict.strEms, "LSK2"+bErrCode.ToString());
            bErrCode = Ems3Data.exeSqlByOleDbCmd(strSql, HashData, oraTrans);
            if (bErrCode > 0)
                bErrCode = 0;
            LogMgr.WriteWarning(Ems3Dict.strEms, "LSK3"+bErrCode.ToString());
            if (bErrCode == -1)
                strErrMsg = "更新状态表状态时失败！";
            return bErrCode;
        }

        public static int getTrDataOfMsg(	//经营范围
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {
            //错误信息
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);

            #region 原经营范围表头查询语句 comment by ccx 2010-5-13
            //strHeadSql.Append("SELECT					 ");
            //strHeadSql.Append("Ems_no					,");						/* 01	:	帐册编号			X(12)	   */
            //strHeadSql.Append("'000000000'		,");								/* 02	:	变更次数(0)			9(9)       */
            //strHeadSql.Append("Pre_ems_no			,");							/* 03	:	预申报帐册编号		X(12)      */
            //strHeadSql.Append("Cop_ems_no			,");							/* 04	:	企业内部编号		X(20)      */
            //strHeadSql.Append("Trade_code			,");							/* 05	:	经营单位代码		X(10)      */
            //strHeadSql.Append("Trade_name			,");							/* 06	:	经营单位名称		X(30)      */
            //strHeadSql.Append("House_no				,");							/* 07	:	仓库编号			X(12)      */
            //strHeadSql.Append("Owner_code			,");							/* 08	:	收货单位代码		X(10)      */
            //strHeadSql.Append("Owner_name			,");							/* 09	:	收货单位名称		X(30)      */
            //strHeadSql.Append("Owner_type			,");							/* 10	:	企业性质			X		   */
            //strHeadSql.Append("Declare_er_type,");									/* 11	:	申请单位类型		X(1)       */
            //strHeadSql.Append("Declare_code		,");								/* 12	:	申请单位代码		X(10)      */
            //strHeadSql.Append("Declare_name		,");								/* 13	:	申请单位名称		X(30)      */
            //strHeadSql.Append("SUBSTR(District_code,1,5),");						/* 14	:	地区代码Z(5)		Z(5)       */
            //strHeadSql.Append("Address				,");							/* 15	:	联系地址			X(30)      */
            //strHeadSql.Append("Phone					,");						/* 16	:	电话号码			X(20)      */
            //strHeadSql.Append("Ems_type				,");							/* 17	:	帐册类型			X(1)       */
            //strHeadSql.Append("Declare_type		,");								/* 18	:	申报类型			X(1)       */
            //strHeadSql.Append("Invest_mode		,");								/* 19	:	投资方式			X(1)       */
            ////strHeadSql.Append("NVL(Trade_mode,'0000'),");							/* 20	:	贸易方式			9(4)       */
            //strHeadSql.Append("ISNULL(Trade_mode,'0000'),");							/* 20	:	贸易方式			9(4)       */
            ////strHeadSql.Append("TO_CHAR(Begin_date,'YYYYMMDD'),");					/* 21	:	开始有效期			Z(8)       */
            //strHeadSql.Append("CONVERT(VARCHAR,Begin_date,112),");					/* 21	:	开始有效期			Z(8)       */
            ////strHeadSql.Append("TO_CHAR(End_date,'YYYYMMDD'),");						/* 22	:	结束有效期			Z(8)       */
            //strHeadSql.Append("CONVERT(VARCHAR,End_date,112),");						/* 22	:	结束有效期			Z(8)       */
            ////strHeadSql.Append("TO_CHAR(NVL(Img_amount,0),'" + N13_5Format + "'),");		/* 23	:	进口总金额			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Img_amount,0),");		/* 23	:	进口总金额			Z(12)9.9(5)*/
            ////strHeadSql.Append("TO_CHAR(NVL(Exg_amount,0),'" + N13_5Format + "'),");		/* 24	:	出口总金额			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Exg_amount,0),");		/* 24	:	出口总金额			Z(12)9.9(5)*/
            ////strHeadSql.Append("TO_CHAR(NVL(Img_weight,0),'" + N13_5Format + "'),");		/* 25	:	进口总重量			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Img_weight,0),");		/* 25	:	进口总重量			Z(12)9.9(5)*/
            ////strHeadSql.Append("TO_CHAR(NVL(Exg_weight,0),'" + N13_5Format + "'),");		/* 26	:	出口总重量			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Exg_weight,0),");		/* 26	:	出口总重量			Z(12)9.9(5)*/
            ////strHeadSql.Append("TO_CHAR(NVL(Img_items,0),'FM999999990'),");			/* 27	:	进口货物项数		Z(8)9      */
            //strHeadSql.Append("ISNULL(Img_items,0),");			/* 27	:	进口货物项数		Z(8)9      */
            ////strHeadSql.Append("TO_CHAR(NVL(Exg_items,0),'FM999999990'),");			/* 28	:	出口货物项数		Z(8)9      */
            //strHeadSql.Append("ISNULL(Exg_items,0),");			/* 28	:	出口货物项数		Z(8)9      */
            //strHeadSql.Append("Ems_appr_no		,");								/* 29	:	批准证编号			X(20)      */
            //strHeadSql.Append("license_no			,");							/* 30	:	许可证编号			X(20)      */
            //strHeadSql.Append("Last_ems_no		,");								/* 31	:	对应上本帐册号		X(12)      */
            //strHeadSql.Append("corr_ems_no		,");								/* 32	:	对应其它帐册号		X(12)      */
            //strHeadSql.Append("Contr_no				,");							/* 33	:	合同号				X(20)      */
            //strHeadSql.Append("Iccard_id			,");							/* 34	:	身份识别号			X(20)      */
            //strHeadSql.Append("Id_card_pwd		,");								/* 35	:	身份识别密码		X(20)      */
            //strHeadSql.Append("Note_1					,");						/* 36	:	备用标志1			X(10)      */
            //strHeadSql.Append("Note_2					,");						/* 37	:	备用标志2			X(10)      */
            ////strHeadSql.Append("TO_CHAR(NVL(Invest_amount,0),'" + N13_5Format + "'),");  /* 38	:	投资金额			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Invest_amount,0),");  /* 38	:	投资金额			Z(12)9.9(5)*/
            ////strHeadSql.Append("TO_CHAR(NVL(Note_amount,0),'" + N13_5Format + "'),");	/* 39	:	备用金额			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Note_amount,0),");	/* 39	:	备用金额			Z(12)9.9(5)*/
            ////strHeadSql.Append("TO_CHAR(NVL(Note_qty,0),'" + N13_5Format + "'),");		/* 40	:	备用数量			Z(12)9.9(5)*/
            //strHeadSql.Append("ISNULL(Note_qty,0),");		/* 40	:	备用数量			Z(12)9.9(5)*/
            //strHeadSql.Append("Note						,");						/* 41	:	备注				X(50)      */
            ////strHeadSql.Append("TO_CHAR(Input_date,'YYYYMMDD')	,");				/* 42	:	录入日期			Z(8)       */
            //strHeadSql.Append("CONVERT(VARCHAR,Input_date,112)	,");				/* 42	:	录入日期			Z(8)       */
            ////strHeadSql.Append("TO_CHAR(NVL(Input_er,0),'FM0000')	,");			/* 43	:	录入员代号			9(4)       */
            //strHeadSql.Append("ISNULL(Input_er,0),");			/* 43	:	录入员代号			9(4)       */
            ////strHeadSql.Append("TO_CHAR(Declare_date,'YYYYMMDD'),");					/* 44	:	申报日期			Z(8)       */
            //strHeadSql.Append("CONVERT(VARCHAR,Declare_date,112),");					/* 44	:	申报日期			Z(8)       */
            ////strHeadSql.Append("TO_CHAR(Declare_date,'HH24MMss'),");					/* 45	:	申报时间			Z(8)       */
            //strHeadSql.Append("CONVERT(VARCHAR,Declare_date,108),");					/* 45	:	申报时间			Z(8)       */
            //strHeadSql.Append("Ems_appr_mark	,");								/* 46	:	其它部门审批标志	X(10)      */
            //strHeadSql.Append("Ems_certify		,");								/* 47	:	其它单证标志		X(10)      */
            ////strHeadSql.Append("TO_CHAR(NVL(Product_ratio,0),'" + N13_5Format + "'),");  /* 48	:	生产能力			Z(12)9.9(5)*/
            //strHeadSql.Append("CONVERT(NUMERIC,ISNULL(Product_ratio,0),'" + N13_5Format + "'),");  /* 48	:	生产能力			Z(12)9.9(5)*/
            //strHeadSql.Append("Modify_mark		 ");								/* 49	:	修改标志			X(1)       */
            //strHeadSql.Append("FROM PRE_EMS3_TR_HEAD WHERE ");
            //strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            //strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
            #endregion

            strHeadSql.Append("SELECT				    ");
            strHeadSql.Append("EMS_NO TR_NO     		 ,");						  //<TR_NO>帐册编号</TR_NO>    
            strHeadSql.Append("COP_EMS_NO COP_TR_NO      ,");	                      //<COP_TR_NO>企业内部编号</COP_TR_NO>       
            strHeadSql.Append("TRADE_CODE                ,");	                      //<TRADE_CODE>经营单位代码</TRADE_CODE>     
            strHeadSql.Append("TRADE_NAME                ,");	                      //<TRADE_NAME>经营单位名称</TRADE_NAME>     
            strHeadSql.Append("HOUSE_NO                  ,");	                      //<HOUSE_NO>仓库编号</HOUSE_NO>            
            strHeadSql.Append("OWNER_CODE                ,");	                      //<OWNER_CODE>加工单位代码</OWNER_CODE>     
            strHeadSql.Append("OWNER_NAME                ,");	                      //<OWNER_NAME>加工单位名称</OWNER_NAME>     
            strHeadSql.Append("OWNER_TYPE                ,");	                      //<OWNER_TYPE>企业性质</OWNER_TYPE>        
            strHeadSql.Append("TRADE_CODE DECLARE_CODE   ,");	                      //<DECLARE_CODE>申请单位代码</DECLARE_CODE> 
            strHeadSql.Append("DECLARE_NAME              ,");	                      //<DECLARE_NAME>申请单位名称</DECLARE_NAME> 
            strHeadSql.Append("'0' DISTRICT_CODE         ,");	                      //<DISTRICT_CODE>地区代码</DISTRICT_CODE>  
            strHeadSql.Append("ADDRESS                   ,");	                      //<ADDRESS>联系地址</ADDRESS>              
            strHeadSql.Append("PHONE                     ,");	                      //<PHONE>电话号码</PHONE>                  
            strHeadSql.Append("'0' EMS_TYPE              ,");	                      //<EMS_TYPE>帐册类型</EMS_TYPE>  0：电子账册经营范围帐册
            strHeadSql.Append("INVEST_MODE               ,");	                      //<INVEST_MODE>投资方式</INVEST_MODE>      
            strHeadSql.Append("'0' TRADE_MODE            ,");	                      //<TRADE_MODE>贸易方式</TRADE_MODE>        
            strHeadSql.Append("CONVERT(VARCHAR,BEGIN_DATE,112) BEGIN_DATE	,");	  //<BEGIN_DATE>开始有效期</BEGIN_DATE>               
            strHeadSql.Append("CONVERT(VARCHAR,END_DATE,112) END_DATE		,");	  //<END_DATE>结束有效期</END_DATE>          	            
            strHeadSql.Append("IMG_AMOUNT                ,");	                      //<IMG_AMOUNT>进口总金额</IMG_AMOUNT>      
            strHeadSql.Append("EXG_AMOUNT                ,");	                      //<EXG_AMOUNT>出口总金额</EXG_AMOUNT>      
            strHeadSql.Append("IMG_WEIGHT                ,");	                      //<IMG_WEIGHT>进口总重量</IMG_WEIGHT>      
            strHeadSql.Append("EXG_WEIGHT                ,");	                      //<EXG_WEIGHT>出口总重量</EXG_WEIGHT>      
            strHeadSql.Append("IMG_ITEMS                 ,");	                      //<IMG_ITEMS>进口货物项数</IMG_ITEMS>       
            strHeadSql.Append("EXG_ITEMS                 ,");	                      //<EXG_ITEMS>出口货物项数</EXG_ITEMS>       
            strHeadSql.Append("EMS_APPR_NO               ,");	           	          //<EMS_APPR_NO>批准文号</EMS_APPR_NO>                           
            strHeadSql.Append("LICENSE_NO                ,");				          //<LICENSE_NO>许可证号</LICENSE_NO>               			  
            strHeadSql.Append("LAST_EMS_NO               ,");						  //<LAST_EMS_NO>对应上本帐册号</LAST_EMS_NO>  					  																																																											
            strHeadSql.Append("CORR_EMS_NO               ,");						  //<CORR_EMS_NO>对应其它帐册号</CORR_EMS_NO>  					  																																																											
            strHeadSql.Append("'0' CONTR_IN              ,");						  //<CONTR_IN>进口合同号</CONTR_IN>          					  																																																											
            strHeadSql.Append("'0' NOTE_1                ,");						  //<NOTE_1>备用标志1</NOTE_1>              					  																																																											
            strHeadSql.Append("NOTE_2                    ,");						  //<NOTE_2>备用标志2</NOTE_2>              					  																																																											
            strHeadSql.Append("INVEST_AMOUNT             ,");						  //<INVEST_AMOUNT>投资金额</INVEST_AMOUNT> 					  																																																											
            strHeadSql.Append("NOTE_AMOUNT               ,");						  //<NOTE_AMOUNT>备用金额</NOTE_AMOUNT>     					  																																																											
            strHeadSql.Append("NOTE_QTY                  ,");						  //<NOTE_QTY>备用数量</NOTE_QTY>           					  																																																											
            strHeadSql.Append("NOTE                      ,");						  //<NOTE>备注</NOTE>                     					  																																																											
            strHeadSql.Append("CONVERT(VARCHAR,INPUT_DATE,112) INPUT_DATE    ,");	  //<INPUT_DATE>录入日期</INPUT_DATE>       
            strHeadSql.Append("'0' INPUT_ER              ,");	                      //<INPUT_ER>录入员代号</INPUT_ER>          
            strHeadSql.Append("CONVERT(VARCHAR,DECLARE_DATE,112) DECLARE_DATE,");	  //<DECLARE_DATE>申报日期</DECLARE_DATE>   
            strHeadSql.Append("'08301020' DECLARE_TIME       ,");	                   //<DECLARE_TIME>申报时间</DECLARE_TIME>   
            strHeadSql.Append("PRODUCT_RATIO             ,");	                      //<PRODUCT_RATIO>生产能力</PRODUCT_RATIO> 
            strHeadSql.Append("'' STORE_VOL              ,");                         //<STORE_VOL>仓库体积</STORE_VOL>         
            strHeadSql.Append("'' STORE_AREA             ,");						  //<STORE_AREA>仓库面积</STORE_AREA>           	
            strHeadSql.Append("I_E_PORT1 I_E_PORT        ,");	                      //<I_E_PORT>进出口岸</I_E_PORT>               
            strHeadSql.Append("'' FOREIGN_CO_NAME           ,");	                      //<FOREIGN_CO_NAME>外商公司</FOREIGN_CO_NAME> 
            strHeadSql.Append("'' AGREE_NO                  ,");	                      //<AGREE_NO>协议号</AGREE_NO>                
            strHeadSql.Append("'0' CUT_MODE                  ,");	                      //<CUT_MODE>征免性质</CUT_MODE>               
            strHeadSql.Append("'' PAY_MODE                  ,");	                      //<PAY_MODE>保税方式</PAY_MODE>               
            strHeadSql.Append("'0' PRODUCE_TYPE              ,");	                      //<PRODUCE_TYPE>加工种类</PRODUCE_TYPE>       
            strHeadSql.Append("'' CONTR_OUT                 ,");	                      //<CONTR_OUT>出口合同号</CONTR_OUT>            
            strHeadSql.Append("'0' APPR_IMG_AMT              ,");	                      //<APPR_IMG_AMT>备案进口总值</APPR_IMG_AMT>     
            strHeadSql.Append("'0' APPR_EXG_AMT              ,");	                      //<APPR_EXG_AMT>备案出口总值</APPR_EXG_AMT>     
            strHeadSql.Append("'' FOREIGN_MGR               ,");	                      //<FOREIGN_MGR>外商经理人</FOREIGN_MGR>        
            strHeadSql.Append("'' TRANS_MODE                ,");	                      //<TRANS_MODE>成交方式</TRANS_MODE>           
            strHeadSql.Append("'' TRADE_COUNTRY             ,");	                      //<TRADE_COUNTRY>起抵地</TRADE_COUNTRY>      
            strHeadSql.Append("'' EQUIP_MODE                ,");	                      //<EQUIP_MODE>引进方式</EQUIP_MODE>           
            strHeadSql.Append("'' IN_RATIO                  ,");	                      //<IN_RATIO>内销比率</IN_RATIO>               
            strHeadSql.Append("'0' EX_CURR                   ,");	                      //<EX_CURR>出口币制</EX_CURR>                 
            strHeadSql.Append("'0' IM_CURR                   ,");	                      //<IM_CURR>进口币制</IM_CURR>                 
            strHeadSql.Append("isnull(MODIFY_MARK,'0') MODIFY_MARK     ,");	              //<MODIFY_MARK>修改标志</MODIFY_MARK>         
            strHeadSql.Append("'0' MASTER_CUSTOMS        ,");	                      //<MASTER_CUSTOMS>主管海关</MASTER_CUSTOMS>   
            strHeadSql.Append("DECLARE_DEP MASTER_FTC       ,");	                      //<MASTER_FTC>主管外经贸部门</MASTER_FTC>        
            strHeadSql.Append("'0' MANAGE_OBJECT             ,");	                      //<MANAGE_OBJECT>管理对象</MANAGE_OBJECT>     
            strHeadSql.Append("'' LIMIT_FLAG                ");	                      //<LIMIT_FLAG>限制类标志</ LIMIT_FLAG >        
            strHeadSql.Append("FROM PRE_EMS3_TR_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");

            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);// DataManager.GetBizConn();
            objRtnRult = new DataTable[3];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[0] != null)
                {
                    objRtnRult[0].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[0].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[0].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[0].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[0].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END
                StringBuilder strExgSql = new StringBuilder();

                #region 原经营范围成品表体查询语句 comment by ccx 2010-5-13
                //strExgSql.Append("SELECT				");
                //strExgSql.Append("''     			 ,");								/*	1.	帐册编号							X(12)      */
                //strExgSql.Append("'0'     ,");											/*	2.	变更次数							Z(8)9      */
                ////strExgSql.Append("TO_CHAR(G_no,'FM999999990') ,");						/*	3.	帐册料件序号						Z(8)9      */
                //strExgSql.Append("CONVERT(NUMERIC,G_no,'FM999999990') ,");						/*	3.	帐册料件序号						Z(8)9      */
                //strExgSql.Append("Cop_g_no     ,");										/*	4.	货号								X(30)      */
                //strExgSql.Append("Code_t_s     ,");										/*	5.	商品编码及附加商品编码				X(16)      */
                //strExgSql.Append("Class_mark   ,");										/*	6.	归类标志							X(1)       */
                //strExgSql.Append("G_name       ,");										/*	7.	商品名称							X(50)      */
                //strExgSql.Append("G_model      ,");										/*	8.	商品规格型号						X(50)      */
                ////strExgSql.Append("NVL(Unit,'000'),");									/*	9.	申报计量单位						9(3)       */
                //strExgSql.Append("ISNULL(Unit,'000'),");									/*	9.	申报计量单位						9(3)       */
                ////strExgSql.Append("NVL(Unit_1,'000')       ,");							/*	10.	法定计量单位						9(3)       */
                //strExgSql.Append("ISNULL(Unit_1,'000')       ,");							/*	10.	法定计量单位						9(3)       */
                ////strExgSql.Append("NVL(Unit_2,'000')       ,");							/*	11.	法定第二单位						9(3)       */
                //strExgSql.Append("ISNULL(Unit_2,'000')       ,");							/*	11.	法定第二单位						9(3)       */
                ////strExgSql.Append("NVL(Country_code,'000') ,");							/*	12.	产销国(地区)						9(3)       */
                //strExgSql.Append("ISNULL(Country_code,'000') ,");							/*	12.	产销国(地区)						9(3)       */
                //strExgSql.Append("Source_mark  ,");										/*	13.	来源标志							X(1)       */
                ////strExgSql.Append("TO_CHAR(NVL(Dec_price,0),'" + N13_5Format + "'),");		/*	14.	企业申报单价						Z(12)9.9(5)*/
                //strExgSql.Append("ISNULL(Dec_price,0),");		/*	14.	企业申报单价						Z(12)9.9(5)*/
                ////strExgSql.Append("NVL(Curr,'000'),");									/*	15.	币制								9(3)       */
                //strExgSql.Append("ISNULL(Curr,'000'),");									/*	15.	币制								9(3)       */
                ////strExgSql.Append("TO_CHAR(NVL(Dec_price_rmb,0),'" + N13_5Format + "'),");	/*	16.	申报单价人民币						Z(12)9.9(5)*/
                //strExgSql.Append("ISNULL(Dec_price_rmb,0),");	/*	16.	申报单价人民币						Z(12)9.9(5)*/
                ////strExgSql.Append("TO_CHAR(NVL(Factor_1,0),'" + N9_9Format + "'),");			/*	17.	法定计量单位比例因子				Z(8)9.9(9) */
                //strExgSql.Append("ISNULL(Factor_1,0),");			/*	17.	法定计量单位比例因子				Z(8)9.9(9) */
                ////strExgSql.Append("TO_CHAR(NVL(Factor_2,0),'" + N9_9Format + "'),");			/*	18.	第二法定计量单位比例因子			Z(8)9.9(9) */
                //strExgSql.Append("ISNULL(Factor_2,0),");			/*	18.	第二法定计量单位比例因子			Z(8)9.9(9) */
                ////strExgSql.Append("TO_CHAR(NVL(Factor_wt,0),'" + N9_9Format + "'),");		/*	19.	重量比例因子						Z(8)9.9(9) */
                //strExgSql.Append("ISNULL(Factor_wt,0),");		/*	19.	重量比例因子						Z(8)9.9(9) */
                ////strExgSql.Append("TO_CHAR(NVL(Factor_rate,0),'FM9990.00000'),");		/*	20.	比例因子浮动比率					Z(3)9.9(5) */
                //strExgSql.Append("ISNULL(Factor_rate,0),");		/*	20.	比例因子浮动比率					Z(3)9.9(5) */
                ////strExgSql.Append("TO_CHAR(NVL(Qty,0),'" + N13_5Format + "'),");				/*	21.	申报进口数量						Z(12)9.9(5)*/
                //strExgSql.Append("ISNULL(Qty,0),");				/*	21.	申报进口数量						Z(12)9.9(5)*/
                ////strExgSql.Append("TO_CHAR(NVL(Max_qty,0),'" + N13_5Format + "'),");			/*	22.	批准最大余量						Z(12)9.9(5)*/
                //strExgSql.Append("ISNULL(Max_qty,0),");			/*	22.	批准最大余量						Z(12)9.9(5)*/
                ////strExgSql.Append("TO_CHAR(NVL(First_qty,0),'" + N13_5Format + "'),");		/*	23.	初始库存数量						Z(12)9.9(5)*/
                //strExgSql.Append("ISNULL(First_qty,0),");		/*	23.	初始库存数量						Z(12)9.9(5)*/
                //strExgSql.Append("I_e_type     ,");										/*	24.	进/出口方式							X(1)       */
                //strExgSql.Append("Use_type     ,");										/*	25.	用途代码							Z(8)9      */
                //strExgSql.Append("Note_1       ,");										/*	26.	备用标志1							X(1)       */
                //strExgSql.Append("Note_2       ,");										/*	27.	备用标志2							X(1)       */
                //strExgSql.Append("Note         ,");										/*	28.	备注								X(10)      */
                //strExgSql.Append("Modify_mark   ");										/*	29.	修改标志							X(1)	   */
                //strExgSql.Append("FROM PRE_EMS3_TR_EXG WHERE ");
                //strExgSql.Append(" MODIFY_MARK!='0' AND ");
                //strExgSql.Append("COP_EMS_NO = {COP_EMS_NO}");
                //strExgSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
                //strExgSql.Append(" ORDER BY G_NO");
                #endregion
 
                strExgSql.Append("SELECT				");
                strExgSql.Append("G_NO                  ,");               	  //<G_NO>成品序号</G_NO>          
                strExgSql.Append("COP_G_NO              ,");                  //<COP_G_NO>成品货号</COP_G_NO>                
                strExgSql.Append("CODE_T_S CODE_T       ,");                  //<CODE_T>商品编码</CODE_T>                    
                strExgSql.Append("'' CODE_S             ,");                  //<CODE_S>附加编码</CODE_S>                    
                strExgSql.Append("G_NAME                ,");                  //<G_NAME>商品名称</G_NAME>                    
                strExgSql.Append("G_MODEL              	,");                  //<G_MODEL>商品规格型号</G_MODEL>                
                strExgSql.Append("'1' UNIT              	,");                  //<UNIT>申报计量单位</UNIT>                      
                strExgSql.Append("'1' UNIT_1              	,");                  //<UNIT_1>法定计量单位</UNIT_1>                  
                strExgSql.Append("UNIT_2              	,");                  //<UNIT_2>法定第二单位</UNIT_2>                  
                strExgSql.Append("COUNTRY_CODE ORIGIN_COUNTRY	,");          //<ORIGIN_COUNTRY>产销国(地区)</ORIGIN_COUNTRY> 
                strExgSql.Append("DEC_PRICE UNIT_PRICE  ,");                  //<UNIT_PRICE>企业申报单价</UNIT_PRICE>          
                strExgSql.Append("CURR              	,");                  //<CURR>币制</CURR>                          
                strExgSql.Append("DEC_PRICE_RMB			,");                  //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                strExgSql.Append("FACTOR_1              ,");                  //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strExgSql.Append("FACTOR_2              ,");                  //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strExgSql.Append("FACTOR_WT             ,");                  //<FACTOR_WT>重量比例因子</FACTOR_WT>            
                strExgSql.Append("FACTOR_RATE           ,");                  //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>      
                strExgSql.Append("QTY I_QTY              	,");                  //<I_QTY>申报进口数量</I_QTY>                    
                strExgSql.Append("MAX_QTY              	,");                  //<MAX_QTY>批准最大余量</MAX_QTY>                
                strExgSql.Append("'' ORIGIN_QTY            ,");                  //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>          
                strExgSql.Append("USE_TYPE              ,");                  //<USE_TYPE>用途代码</USE_TYPE>                
                strExgSql.Append("NOTE_1              	,");                  //<NOTE_1>备用标志1</NOTE_1>                   
                strExgSql.Append("NOTE_2              	,");                  //<NOTE_2>备用标志2</NOTE_2>                   
                strExgSql.Append("NOTE              	,");                  //<NOTE>备注</NOTE>                          
                strExgSql.Append("MODIFY_MARK           ,");                  //<MODIFY_MARK>修改标志</MODIFY_MARK>          
                strExgSql.Append("'' APPR_AMT              ,");                  //<APPR_AMT>总价</APPR_AMT>                  
                strExgSql.Append("'' G_ENAME              	,");                 //<G_ENAME>英文名称</G_ENAME>                  
                strExgSql.Append("'' G_EMODEL              ,");                  //<G_EMODEL>英文规格型号</G_EMODEL>              
                strExgSql.Append("'' CLASS_NOTE            ,");                  //<CLASS_NOTE>归类说明</CLASS_NOTE>            
                strExgSql.Append("'' COP_UNIT              ,");                  //<COP_UNIT>企业自编计量单位</COP_UNIT>            
                strExgSql.Append("'' COP_FACTOR            ,");                  //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>    
                strExgSql.Append("'' DUTY_MODE             ");                  //<DUTY_MODE>征免方式</DUTY_MODE>              
                strExgSql.Append("FROM PRE_EMS3_TR_EXG WHERE ");
                strExgSql.Append(" MODIFY_MARK!='0' AND ");
                strExgSql.Append("COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" ORDER BY G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[1] != null)
                {
                    objRtnRult[1].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[1].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[1].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[1].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[1].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END

                StringBuilder strImgSql = new StringBuilder();

                #region 原经营范围料件表体查询语句 comment by ccx 2010-5-13
                //strImgSql.Append("SELECT				");
                //strImgSql.Append("''     			 ,");								/*	1.	帐册编号							X(12)	   */
                //strImgSql.Append("'0'     ,");											/*	2.	变更次数							Z(8)9      */
                ////strImgSql.Append("TO_CHAR(G_no,'FM999999990') ,");						/*	3.	帐册料件序号						Z(8)9      */
                //strImgSql.Append("G_no,");						/*	3.	帐册料件序号						Z(8)9      */
                //strImgSql.Append("Cop_g_no     ,");										/*	4.	货号								X(30)      */
                //strImgSql.Append("Code_t_s     ,");										/*	5.	商品编码及附加商品编码				X(16)      */
                //strImgSql.Append("Class_mark   ,");										/*	6.	归类标志							X(1)       */
                //strImgSql.Append("G_name       ,");										/*	7.	商品名称							X(50)      */
                //strImgSql.Append("G_model      ,");										/*	8.	商品规格型号						X(50)      */
                ////strImgSql.Append("NVL(Unit,'000'),");									/*	9.	申报计量单位						9(3)       */
                //strImgSql.Append("ISNULL(Unit,'000'),");									/*	9.	申报计量单位						9(3)       */
                ////strImgSql.Append("NVL(Unit_1,'000')       ,");							/*	10.	法定计量单位						9(3)       */
                //strImgSql.Append("ISNULL(Unit_1,'000')       ,");							/*	10.	法定计量单位						9(3)       */
                ////strImgSql.Append("NVL(Unit_2,'000')       ,");							/*	11.	法定第二单位						9(3)       */
                //strImgSql.Append("ISNULL(Unit_2,'000')       ,");							/*	11.	法定第二单位						9(3)       */
                ////strImgSql.Append("NVL(Country_code,'000') ,");							/*	12.	产销国(地区)						9(3)       */
                //strImgSql.Append("ISNULL(Country_code,'000') ,");							/*	12.	产销国(地区)						9(3)       */
                //strImgSql.Append("Source_mark  ,");										/*	13.	来源标志							X(1)       */
                ////strImgSql.Append("TO_CHAR(NVL(Dec_price,0),'" + N13_5Format + "'),");		/*	14.	企业申报单价						Z(12)9.9(5)*/
                //strImgSql.Append("ISNULL(Dec_price,0),");		/*	14.	企业申报单价						Z(12)9.9(5)*/
                ////strImgSql.Append("NVL(Curr,'000'),");									/*	15.	币制								9(3)       */
                //strImgSql.Append("ISNULL(Curr,'000'),");									/*	15.	币制								9(3)       */
                ////strImgSql.Append("TO_CHAR(NVL(Dec_price_rmb,0),'" + N13_5Format + "'),");	/*	16.	申报单价人民币						Z(12)9.9(5)*/
                //strImgSql.Append("ISNULL(Dec_price_rmb,0),");	/*	16.	申报单价人民币						Z(12)9.9(5)*/
                ////strImgSql.Append("TO_CHAR(NVL(Factor_1,0),'" + N9_9Format + "'),");			/*	17.	法定计量单位比例因子				Z(8)9.9(9) */
                //strImgSql.Append("ISNULL(Factor_1,0),");			/*	17.	法定计量单位比例因子				Z(8)9.9(9) */
                ////strImgSql.Append("TO_CHAR(NVL(Factor_2,0),'" + N9_9Format + "'),");			/*	18.	第二法定计量单位比例因子			Z(8)9.9(9) */
                //strImgSql.Append("ISNULL(Factor_2,0),");			/*	18.	第二法定计量单位比例因子			Z(8)9.9(9) */
                ////strImgSql.Append("TO_CHAR(NVL(Factor_wt,0),'" + N9_9Format + "'),");		/*	19.	重量比例因子						Z(8)9.9(9) */
                //strImgSql.Append("ISNULL(Factor_wt,0),");		/*	19.	重量比例因子						Z(8)9.9(9) */
                ////strImgSql.Append("TO_CHAR(NVL(Factor_rate,0),'FM9990.00000'),");		/*	20.	比例因子浮动比率					Z(3)9.9(5) */
                //strImgSql.Append("ISNULL(Factor_rate,0),");		/*	20.	比例因子浮动比率					Z(3)9.9(5) */
                ////strImgSql.Append("TO_CHAR(NVL(Qty,0),'" + N13_5Format + "'),");				/*	21.	申报进口数量						Z(12)9.9(5)*/
                //strImgSql.Append("ISNULL(Qty,0),");				/*	21.	申报进口数量						Z(12)9.9(5)*/
                ////strImgSql.Append("TO_CHAR(NVL(Max_qty,0),'" + N13_5Format + "'),");			/*	22.	批准最大余量						Z(12)9.9(5)*/
                //strImgSql.Append("ISNULL(Max_qty,0),");			/*	22.	批准最大余量						Z(12)9.9(5)*/
                ////strImgSql.Append("TO_CHAR(NVL(First_qty,0),'" + N13_5Format + "'),");		/*	23.	初始库存数量						Z(12)9.9(5)*/
                //strImgSql.Append("ISNULL(First_qty,0),");		/*	23.	初始库存数量						Z(12)9.9(5)*/
                //strImgSql.Append("I_e_type     ,");										/*	24.	进/出口方式							X(1)       */
                //strImgSql.Append("Use_type     ,");										/*	25.	用途代码							Z(8)9      */
                //strImgSql.Append("Note_1       ,");										/*	26.	备用标志1							X(1)       */
                //strImgSql.Append("Note_2       ,");										/*	27.	备用标志2							X(1)       */
                //strImgSql.Append("Note         ,");										/*	28.	备注								X(10)      */
                //strImgSql.Append("Modify_mark   ");										/*	29.	修改标志							X(1)	   */
                //strImgSql.Append("FROM PRE_EMS3_TR_IMG WHERE ");
                //strImgSql.Append(" MODIFY_MARK!='0' AND ");
                //strImgSql.Append("COP_EMS_NO = {COP_EMS_NO}");
                //strImgSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
                //strImgSql.Append(" ORDER BY G_NO");
                #endregion

                strImgSql.Append("SELECT				");
                strImgSql.Append("G_NO                  ,");               	  //<G_NO>料件序号</G_NO>          
                strImgSql.Append("COP_G_NO              ,");                  //<COP_G_NO>料件货号</COP_G_NO>                
                strImgSql.Append("CODE_T_S CODE_T       ,");                  //<CODE_T>商品编码</CODE_T>                    
                strImgSql.Append("'' CODE_S             ,");                  //<CODE_S>附加编码</CODE_S>                    
                strImgSql.Append("G_NAME                ,");                  //<G_NAME>商品名称</G_NAME>                    
                strImgSql.Append("G_MODEL              	,");                  //<G_MODEL>商品规格型号</G_MODEL>                
                strImgSql.Append("'1' UNIT              	,");                  //<UNIT>申报计量单位</UNIT>                      
                strImgSql.Append("'1' UNIT_1              	,");                  //<UNIT_1>法定计量单位</UNIT_1>                  
                strImgSql.Append("UNIT_2              	,");                  //<UNIT_2>法定第二单位</UNIT_2>                  
                strImgSql.Append("COUNTRY_CODE ORIGIN_COUNTRY	,");          //<ORIGIN_COUNTRY>产销国(地区)</ORIGIN_COUNTRY> 
                strImgSql.Append("DEC_PRICE UNIT_PRICE  ,");                  //<UNIT_PRICE>企业申报单价</UNIT_PRICE>          
                strImgSql.Append("CURR              	,");                  //<CURR>币制</CURR>                          
                strImgSql.Append("DEC_PRICE_RMB			,");                  //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                strImgSql.Append("FACTOR_1              ,");                  //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strImgSql.Append("FACTOR_2              ,");                  //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strImgSql.Append("FACTOR_WT             ,");                  //<FACTOR_WT>重量比例因子</FACTOR_WT>            
                strImgSql.Append("FACTOR_RATE           ,");                  //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>      
                strImgSql.Append("QTY I_QTY              	,");                  //<I_QTY>申报进口数量</I_QTY>                    
                strImgSql.Append("MAX_QTY              	,");                  //<MAX_QTY>批准最大余量</MAX_QTY>                
                strImgSql.Append("'' ORIGIN_QTY            ,");                  //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>          
                strImgSql.Append("USE_TYPE              ,");                  //<USE_TYPE>用途代码</USE_TYPE>                
                strImgSql.Append("NOTE_1              	,");                  //<NOTE_1>备用标志1</NOTE_1>                   
                strImgSql.Append("NOTE_2              	,");                  //<NOTE_2>备用标志2</NOTE_2>                   
                strImgSql.Append("NOTE              	,");                  //<NOTE>备注</NOTE>                          
                strImgSql.Append("MODIFY_MARK           ,");                  //<MODIFY_MARK>修改标志</MODIFY_MARK>          
                strImgSql.Append("'' APPR_AMT              ,");                  //<APPR_AMT>总价</APPR_AMT>                  
                strImgSql.Append("'' G_ENAME              	,");                 //<G_ENAME>英文名称</G_ENAME>                  
                strImgSql.Append("'' G_EMODEL              ,");                  //<G_EMODEL>英文规格型号</G_EMODEL>              
                strImgSql.Append("'' CLASS_NOTE            ,");                  //<CLASS_NOTE>归类说明</CLASS_NOTE>            
                strImgSql.Append("'' COP_UNIT              ,");                  //<COP_UNIT>企业自编计量单位</COP_UNIT>            
                strImgSql.Append("'' COP_FACTOR            ,");                  //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>    
                strImgSql.Append("'' DUTY_MODE             ");                  //<DUTY_MODE>征免方式</DUTY_MODE>              
                strImgSql.Append("FROM PRE_EMS3_TR_IMG WHERE ");
                strImgSql.Append(" MODIFY_MARK!='0' AND ");
                strImgSql.Append("COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" ORDER BY G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[2] != null)
                {
                    objRtnRult[2].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[2].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[2].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[2].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[2].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END

                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
               //   DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
                DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }

        #region 归并关系
        public static int getMrDataOfMsg(//归并关系
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {//错误信息
            /*
             * 归并关系表头
             */
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);

            #region 原归并关系表头查询语句 comment by ccx 2010-5-18
            //strHeadSql.Append("select									");
            //strHeadSql.Append("Ems_no          				,			");         /*1.		帐册编号          X(12)       */
            //strHeadSql.Append("'000000000'     				,			");         /*2.		#变更次数         9(9)        */
            //strHeadSql.Append("Pre_ems_no      				,			");         /*3.		预申报帐册编号    X(12)       */
            //strHeadSql.Append("Cop_ems_no      				,			");         /*4.		企业内部编号      X(20)       */
            //strHeadSql.Append("Trade_code      				,			");         /*5.		经营单位代码      X(10)       */
            //strHeadSql.Append("Trade_name      				,			");         /*6.		经营单位名称      X(30)       */
            //strHeadSql.Append("House_no        				,			");         /*7.		仓库编号          X(12)       */
            //strHeadSql.Append("Owner_code      				,			");         /*8.		收货单位代码      X(10)       */
            //strHeadSql.Append("Owner_name      				,			");         /*9.		收货单位名称      X(30)       */
            //strHeadSql.Append("Owner_type      				,			");         /*10.		企业性质          X           */
            //strHeadSql.Append("Declare_er_type 				,			");         /*11.		申请单位类型      X(1)        */
            //strHeadSql.Append("Declare_code    				,			");         /*12.		申请单位代码      X(10)       */
            //strHeadSql.Append("Declare_name    				,			");         /*13.		申请单位名称      X(30)       */
            //strHeadSql.Append("SUBSTR(District_code,1,5)	,			");         /*14.		地区代码          Z(5)        */
            //strHeadSql.Append("Address         				,			");         /*15.		联系地址          X(30)       */
            //strHeadSql.Append("Phone           				,			");         /*16.		电话号码          X(20)       */
            //strHeadSql.Append("Ems_type        				,			");         /*17.		帐册类型          X(1)        */
            //strHeadSql.Append("Declare_type    				,			");         /*18.		申报类型          X(1)        */
            //strHeadSql.Append("Invest_mode     				,			");         /*19.		投资方式          X(1)        */
            //strHeadSql.Append("NVL(Trade_mode,'0000')		,			");         /*20.		贸易方式          9(4)        */
            //strHeadSql.Append("TO_CHAR(Begin_date,'YYYYMMDD'),			");         /*21.		开始有效期        Z(8)        */
            //strHeadSql.Append("TO_CHAR(End_date,'YYYYMMDD') ,			");         /*22.		结束有效期        Z(8)        */
            //strHeadSql.Append("TO_CHAR(NVL(Img_amount,0),'" + N13_5Format + "'),");     /*23.		进口总金额        Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_amount,0),'" + N13_5Format + "'),");		/*24		出口总金额		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Img_weight,0),'" + N13_5Format + "'),");		/*25		进口总重量		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_weight,0),'" + N13_5Format + "'),");		/*26		出口总重量		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Img_items,0),'FM999999990'),");			/*27		进口货物项数	  Z(8)9       */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_items,0),'FM999999990'),");			/*28		出口货物项数	  Z(8)9       */
            //strHeadSql.Append("Ems_appr_no     				,			");         /*29.		批准证编号        X(20)       */
            //strHeadSql.Append("license_no      				,			");         /*30.		许可证编号        X(20)       */
            //strHeadSql.Append("Last_ems_no     				,			");         /*31.		对应上本帐册号    X(12)       */
            //strHeadSql.Append("corr_ems_no     				,			");         /*32.		对应其它帐册号    X(12)       */
            //strHeadSql.Append("Contr_no        				,			");         /*33.		合同号            X(20)       */
            //strHeadSql.Append("Iccard_id       				,			");         /*34.		身份识别号        X(20)       */
            //strHeadSql.Append("Id_card_pwd     				,			");         /*35.		身份识别密码      X(20)       */
            //strHeadSql.Append("Note_1          				,			");         /*36.		备用标志1         X(10)       */
            //strHeadSql.Append("Note_2          				,			");         /*37.		备用标志2         X(10)       */
            //strHeadSql.Append("TO_CHAR(NVL(Invest_amount,0),'" + N13_5Format + "'),");  /*38		投资金额		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Note_amount,0),'" + N13_5Format + "'),");    /*39		备用金额		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Note_qty,0),'" + N13_5Format + "'),");	    /*40		备用数量		  Z(12)9.9(5) */
            //strHeadSql.Append("Note            				,			");         /*41.		备注              X(50)       */
            //strHeadSql.Append("TO_CHAR(Input_date,'YYYYMMDD'),			");         /*42.		录入日期          Z(8)        */
            //strHeadSql.Append("TO_CHAR(NVL(Input_er,0),'FM0000'),		");         /*43.		录入员代号        9(4)        */
            //strHeadSql.Append("TO_CHAR(Declare_date,'YYYYMMDD'),			");         /*44.		申报日期          Z(8)        */
            //strHeadSql.Append("TO_CHAR(Declare_date,'HH24MMss'),		");         /*45.		申报时间          Z(8)        */
            //strHeadSql.Append("Ems_appr_mark   				,			");         /*46.		其它部门审批标志  X(10)       */
            //strHeadSql.Append("Ems_certify     				,			");         /*47.		其它单证标志      X(10)       */
            //strHeadSql.Append("TO_CHAR(NVL(Product_ratio,0),'" + N13_5Format + "'),");  /*48.		生产能力          Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(STORE_VOL,0),'" + N13_5Format + "'),");      /*49.		仓库体积          z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(STORE_AREA,0),'" + N13_5Format + "'),");     /*50.		仓库面积          z(12)9.9(5) */
            //strHeadSql.Append("I_E_PORT                     ,			");         /*51.		进出口岸          X(255)      */
            //strHeadSql.Append("FOREIGN_CO_NAME              ,			");         /*52.		外商公司          X(255)      */
            //strHeadSql.Append("AGREE_NO                     ,			");         /*53.		协议号            X(32)       */
            //strHeadSql.Append("CUT_MODE                     ,			");         /*54.		征免性质          X(4)        */
            //strHeadSql.Append("PAY_MODE                     ,			");         /*55.		保税方式          X(1)        */
            //strHeadSql.Append("PRODUCE_TYPE                 ,			");         /*56.		加工种类          X(2)        */
            //strHeadSql.Append("CONTR_OUT                    ,			");         /*57.		出口合同号        X(32)       */
            //strHeadSql.Append("Modify_mark                				");	        /*58.		修改标志		  X(1)        */
            //strHeadSql.Append("FROM PRE_EMS3_HEAD WHERE ");
            //strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            //strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
            #endregion

            strHeadSql.Append("select                           ");
            strHeadSql.Append("COP_EMS_NO,");		        //<COP_ENT_NO>企业内部编号</COP_ENT_NO>       
            strHeadSql.Append("TRADE_CODE,");                //<TRADE_CODE>经营单位编码</TRADE_CODE>       
            strHeadSql.Append("TRADE_NAME,");                //<TRADE_NAME>经营单位名称</TRADE_NAME>       
            strHeadSql.Append("OWNER_CODE,");                //<OWNER_CODE>加工单位编码</OWNER_CODE>       
            strHeadSql.Append("OWNER_NAME,");                //<OWNER_NAME>加工单位名称</OWNER_NAME>
            strHeadSql.Append("OWNER_CODE AS DECLARE_CODE,");                //<OWNER_CODE>加工单位编码</OWNER_CODE>       
            strHeadSql.Append("OWNER_NAME AS DECLARE_NAME,");                //<OWNER_NAME>加工单位名称</OWNER_NAME> 
            strHeadSql.Append("EMS_TYPE,");                //<EMS_TYPE>手册类型</EMS_TYPE>       
            strHeadSql.Append("DECLARE_TYPE,");                //<DECLARE_TYPE>申报类型</DECLARE_TYPE> 
            strHeadSql.Append("TRADE_MODE,");      //<TRADE_MODE>贸易方式</TRADE_MODE>  
            strHeadSql.Append("CONVERT(VARCHAR,BEGIN_DATE,112) BEGIN_DATE,");      //<BEGIN_DATE>开始有效期</BEGIN_DATE>       
            strHeadSql.Append("CONVERT(VARCHAR,END_DATE,112) END_DATE,");      //<END_DATE>结束有效期</END_DATE>  
            strHeadSql.Append("EMS_APPR_NO,");      //<EMS_APPR_NO>批准证编号</EMS_APPR_NO>
            strHeadSql.Append("CONTR_NO,");      //<CONTR_IN>进口合同号</CONTR_IN> 
            strHeadSql.Append("ICCARD_ID,");                //<ICCARD_ID>身份识别号#</ICCARD_ID>  
            strHeadSql.Append("CORR_EMS_NO,");      //<CORR_EMS_NO>对应其它帐册号</CORR_EMS_NO> 
            strHeadSql.Append("NOTE_1,");      //<NOTE_1>BOM归并</NOTE_1>               
            strHeadSql.Append("NOTE_2,");      //<NOTE_2>损耗率模式</NOTE_2>   
            strHeadSql.Append("NOTE_AMOUNT,");      //<NOTE_AMOUNT>备用金额</NOTE_AMOUNT>          
            strHeadSql.Append("NOTE_QTY,");      //<NOTE_QTY>备用数量</NOTE_QTY> 
            strHeadSql.Append("CONVERT(VARCHAR,INPUT_DATE,112) INPUT_DATE,");      //<INPUT_DATE>录入日期</INPUT_DATE>            
            strHeadSql.Append("INPUT_ER,");      //<INPUT_ER>录入员代号</INPUT_ER>               
            strHeadSql.Append("PRODUCT_RATIO,");		        //<PRODUCT_RATIO>生产能力</PRODUCT_RATIO>       
            strHeadSql.Append("STORE_AREA,");                //<STORE_AREA>仓库面积</STORE_AREA>       
            strHeadSql.Append("I_E_PORT,");                //<I_E_PORT>进出口岸</I_E_PORT>       
            strHeadSql.Append("FOREIGN_CO_NAME,");                //<FOREIGN_CO_NAME>外商公司</FOREIGN_CO_NAME>       
            strHeadSql.Append("AGREE_NO,");                //<AGREE_NO>协议号</AGREE_NO>     
            strHeadSql.Append("CUT_MODE,");                //<CUT_MODE>征免规定</CUT_MODE> 
            strHeadSql.Append("PAY_MODE,");      //<PAY_MODE>保税方式</PAY_MODE>         
            strHeadSql.Append("PRODUCE_TYPE,");   //<PRODUCE_TYPE>加工种类</PRODUCE_TYPE>     
            strHeadSql.Append("CONTR_OUT,");                //<CONTR_OUT>出口合同</CONTR_OUT>   
            strHeadSql.Append("STORE_VOL ,");                //<STORE_VOL>仓库体积</STORE_VOL>          
            strHeadSql.Append("NOTE           ");                //<NOTE>备注</NOTE>                       
            strHeadSql.Append("FROM PRE_EMS3_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");

            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[8];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }


                /*
                 * 归并关系归并后料件
                 */
                StringBuilder strImgSql = new StringBuilder();

                #region 原归并关系归并后料件查询语句 comment by ccx 2010-5-18
                //strImgSql.Append("select						");
                //strImgSql.Append("A.Ems_no         		,	");  								     /*	1.	帐册编号                  X(12)        */
                //strImgSql.Append("'0'        		,	");  										 /*	2.	变更次数               	  Z(8)9        */
                //strImgSql.Append("TO_CHAR(B.G_no,'FM999999990')           		,	");  			 /*	3.	帐册料件序号              Z(8)9        */
                //strImgSql.Append("B.Cop_g_no       		,	");  								     /*	4.	货号                      X(30)        */
                //strImgSql.Append("B.Code_t_s       		,	");  								     /*	5.	商品编码及附加商品编码    X(16)        */
                //strImgSql.Append("B.Class_mark     		,	");  								     /*	6.	归类标志                  X(1)         */
                //strImgSql.Append("B.G_name         		,	");  								     /*	7.	商品名称                  X(50)        */
                //strImgSql.Append("B.G_model        		,	");  								     /*	8.	商品规格型号              X(50)        */
                //strImgSql.Append("NVL(B.Unit,'000')           		,	");  						 /*	9.	申报计量单位              9(3)         */
                //strImgSql.Append("NVL(B.Unit_1,'000')         		,	");  						 /*	10.	法定计量单位              9(3)         */
                //strImgSql.Append("NVL(B.Unit_2,'000')         		,	");  						 /*	11.	法定第二单位              9(3)         */
                //strImgSql.Append("NVL(B.Country_code,'000')   		,	");  						 /*	12.	产销国(地区)              9(3)         */
                //strImgSql.Append("B.Source_mark    		,	");  								     /*	13.	来源标志                  X(1)         */
                //strImgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "')      		,	");  /*	14.	企业申报单价              Z(12)9.9(5)  */
                //strImgSql.Append("NVL(B.Curr,'000')           		,	");  						 /*	15.	币制                      9(3)         */
                //strImgSql.Append("TO_CHAR(NVL(B.Dec_price_rmb,0),'" + N13_5Format + "')  		,	");  /*	16.	申报单价人民币            Z(12)9.9(5)  */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_1,0),'" + N9_9Format + "')       		,	");  /*	17.	法定计量单位比例因子      Z(8)9.9(9)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_2,0),'" + N9_9Format + "')       		,	");  /*	18.	第二法定计量单位比例因子  Z(8)9.9(9)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_wt,0),'" + N9_9Format + "')      		,	");      /*	19.	重量比例因子              Z(8)9.9(9)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_rate,0),'FM9990.00000')    		,	");  	 /*	20.	比例因子浮动比率          Z(3)9.9(5)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "')            		,	");  /*	21.	申报进口数量              Z(12)9.9(5)  */
                //strImgSql.Append("TO_CHAR(NVL(B.Max_qty,0),'" + N13_5Format + "')        		,	");  /*	22.	批准最大余量              Z(12)9.9(5)  */
                //strImgSql.Append("TO_CHAR(NVL(B.First_qty,0),'" + N13_5Format + "')      		,	");  /*	23.	初始库存数量              Z(12)9.9(5)  */
                //strImgSql.Append("B.I_e_type           	,	");  								     /*	24.	进/出口方式               X(1)         */
                //strImgSql.Append("B.Use_type           	,	");  								     /*	25.	用途代码                  Z(8)9        */
                //strImgSql.Append("B.Note_1             	,	");  								     /*	26.	备用标志1                 X(1)         */
                //strImgSql.Append("B.Note_2             	,	");  								     /*	27.	备用标志2                 X(1)         */
                //strImgSql.Append("B.Note               	,	");  								     /*	28.	备注                      X(10)        */
                //strImgSql.Append("B.Modify_mark        	    ");  								     /*	29	修改标志                  X(1)         */
                //strImgSql.Append("from                       	");
                //strImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_IMG B WHERE ");
                //strImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                //strImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strImgSql.Append(" ORDER BY B.G_NO");
                #endregion

                strImgSql.Append("select						    ");
                strImgSql.Append("B.G_NO G_NO        		    	,");		  //<G_NO>料件序号</G_NO>                        
                strImgSql.Append("B.COP_G_NO COP_G_NO        		,");          //<COP_G_NO>料件货号</COP_G_NO>                
                strImgSql.Append("substr(B.CODE_T_S,0,9) CODE_T     ,");          //<CODE_T>商品编码</CODE_T>                    
                strImgSql.Append("substr(B.CODE_T_S,9) CODE_S       ,");          //<CODE_S>附加编码</CODE_S>                    
                strImgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                strImgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>商品规格型号</G_MODEL>                
                strImgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                strImgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                strImgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                strImgSql.Append("B.COUNTRY_CODE COUNTRY_CODE	,");          //<COUNTRY_CODE>产销国(地区)</COUNTRY_CODE> 
                strImgSql.Append("B.DEC_PRICE DEC_PRICE         	,");          //<DEC_PRICE>企业申报单价</DEC_PRICE>          
                strImgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                strImgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                strImgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strImgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strImgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>              
                strImgSql.Append("B.QTY QTY         		        ,");          //<QTY>申报进口数量</QTY>                    
                strImgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                strImgSql.Append("B.FIRST_QTY FIRST_QTY         	,");          //<FIRST_QTY>初始库存数量</FIRST_QTY>                     
                strImgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                strImgSql.Append("B.NOTE_2 NOTE_2         		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                strImgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>                          
                strImgSql.Append("B.MODIFY_MARK MODIFY_MARK         ,");          //<MODIFY_MARK>修改标志</MODIFY_MARK>          
                strImgSql.Append("B.DUTY_MODE         	");           //<DUTY_MODE>征免方式</DUTY_MODE>              
                strImgSql.Append("from                       	    ");
                strImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_IMG B WHERE ");
                strImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" ORDER BY B.G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系归并后成品
                 */
                StringBuilder strExgSql = new StringBuilder();

                #region 原归并关系归并后成品查询语句 comment by ccx 2010-5-18
                //strExgSql.Append("select						");
                //strExgSql.Append("A.Ems_no         		,	");  								     /*	1.	帐册编号                  X(12)        */
                //strExgSql.Append("'0'        		,	");  										 /*	2.	变更次数               	  Z(8)9        */
                //strExgSql.Append("TO_CHAR(B.G_no,'FM999999990')           		,	");  			 /*	3.	帐册料件序号              Z(8)9        */
                //strExgSql.Append("B.Cop_g_no       		,	");  								     /*	4.	货号                      X(30)        */
                //strExgSql.Append("B.Code_t_s       		,	");  								     /*	5.	商品编码及附加商品编码    X(16)        */
                //strExgSql.Append("B.Class_mark     		,	");  								     /*	6.	归类标志                  X(1)         */
                //strExgSql.Append("B.G_name         		,	");  								     /*	7.	商品名称                  X(50)        */
                //strExgSql.Append("B.G_model        		,	");  								     /*	8.	商品规格型号              X(50)        */
                //strExgSql.Append("NVL(B.Unit,'000')           		,	");  						 /*	9.	申报计量单位              9(3)         */
                //strExgSql.Append("NVL(B.Unit_1,'000')         		,	");  						 /*	10.	法定计量单位              9(3)         */
                //strExgSql.Append("NVL(B.Unit_2,'000')         		,	");  						 /*	11.	法定第二单位              9(3)         */
                //strExgSql.Append("NVL(B.Country_code,'000')   		,	");  						 /*	12.	产销国(地区)              9(3)         */
                //strExgSql.Append("B.Source_mark    		,	");  								     /*	13.	来源标志                  X(1)         */
                //strExgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "')      		,	");  /*	14.	企业申报单价              Z(12)9.9(5)  */
                //strExgSql.Append("NVL(B.Curr,'000')           		,	");  						 /*	15.	币制                      9(3)         */
                //strExgSql.Append("TO_CHAR(NVL(B.Dec_price_rmb,0),'" + N13_5Format + "')  		,	");  /*	16.	申报单价人民币            Z(12)9.9(5)  */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_1,0),'" + N9_9Format + "')       		,	");  /*	17.	法定计量单位比例因子      Z(8)9.9(9)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_2,0),'" + N9_9Format + "')       		,	");  /*	18.	第二法定计量单位比例因子  Z(8)9.9(9)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_wt,0),'" + N9_9Format + "')      		,	");      /*	19.	重量比例因子              Z(8)9.9(9)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_rate,0),'FM9990.00000')    		,	");  	 /*	20.	比例因子浮动比率          Z(3)9.9(5)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "')            		,	");  /*	21.	申报进口数量              Z(12)9.9(5)  */
                //strExgSql.Append("TO_CHAR(NVL(B.Max_qty,0),'" + N13_5Format + "')        		,	");  /*	22.	批准最大余量              Z(12)9.9(5)  */
                //strExgSql.Append("TO_CHAR(NVL(B.First_qty,0),'" + N13_5Format + "')      		,	");  /*	23.	初始库存数量              Z(12)9.9(5)  */
                //strExgSql.Append("B.I_e_type           	,	");  								     /*	24.	进/出口方式               X(1)         */
                //strExgSql.Append("B.Use_type           	,	");  								     /*	25.	用途代码                  Z(8)9        */
                //strExgSql.Append("B.Note_1             	,	");  								     /*	26.	备用标志1                 X(1)         */
                //strExgSql.Append("B.Note_2             	,	");  								     /*	27.	备用标志2                 X(1)         */
                //strExgSql.Append("B.Note               	,	");  								     /*	28.	备注                      X(10)        */
                //strExgSql.Append("B.Modify_mark        	    ");  								     /*	29	修改标志                  X(1)         */
                //strExgSql.Append("from                       	");
                //strExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_EXG B WHERE ");
                //strExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                //strExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strExgSql.Append(" ORDER BY B.G_NO");
                #endregion

                strExgSql.Append("select					");
                strExgSql.Append("B.G_NO G_NO,");		  //<G_NO>料件序号</G_NO>                        
                strExgSql.Append("B.COP_G_NO COP_G_NO,");          //<COP_G_NO>料件货号</COP_G_NO>                
                strExgSql.Append("substr(B.CODE_T_S,0,9) CODE_T,");          //<CODE_T>商品编码</CODE_T>                    
                strExgSql.Append("substr(B.CODE_T_S,9) CODE_S,");          //<CODE_S>附加编码</CODE_S>                    
                strExgSql.Append("B.G_NAME G_NAME,");          //<G_NAME>商品名称</G_NAME>                    
                strExgSql.Append("B.G_MODEL G_MODEL,");          //<G_MODEL>商品规格型号</G_MODEL>                
                strExgSql.Append("B.UNIT UNIT,");          //<UNIT>申报计量单位</UNIT>                      
                strExgSql.Append("B.UNIT_1 UNIT_1,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                strExgSql.Append("B.UNIT_2 UNIT_2,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                strExgSql.Append("B.COUNTRY_CODE COUNTRY_CODE,");          //<COUNTRY_CODE>产销国(地区)</COUNTRY_CODE> 
                strExgSql.Append("B.DEC_PRICE DEC_PRICE,");          //<DEC_PRICE>企业申报单价</DEC_PRICE>          
                strExgSql.Append("B.CURR CURR,");          //<CURR>币制</CURR>                          
                strExgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                strExgSql.Append("B.FACTOR_1 FACTOR_1,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strExgSql.Append("B.FACTOR_2 FACTOR_2,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strExgSql.Append("B.FACTOR_WT FACTOR_WT,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>             
                strExgSql.Append("B.QTY QTY,");          //<QTY>申报进口数量</QTY>                    
                strExgSql.Append("B.MAX_QTY MAX_QTY,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                strExgSql.Append("B.FIRST_QTY FIRST_QTY,");          //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>                     
                strExgSql.Append("B.NOTE_1 NOTE_1,");          //<NOTE_1>备用标志1</NOTE_1>                   
                strExgSql.Append("'1' NOTE_2,");          //<NOTE_2>备用标志2</NOTE_2>                   
                strExgSql.Append("B.NOTE NOTE,");          //<NOTE>备注</NOTE>                          
                strExgSql.Append("B.MODIFY_MARK MODIFY_MARK,");          //<MODIFY_MARK>修改标志</MODIFY_MARK>          
                strExgSql.Append("B.DUTY_MODE       ");           //<DUTY_MODE>征免方式</DUTY_MODE>              
                strExgSql.Append("from              ");
                strExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_EXG B WHERE ");
                strExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" ORDER BY B.G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系归并前料件
                 */
                StringBuilder strOrgImgSql = new StringBuilder();

                #region 原归并关系归并前料件查询语句 comment by ccx 2010-5-18
                //strOrgImgSql.Append("select						");
                //strOrgImgSql.Append("A.Ems_no         		,	");  								     /*	1.	帐册编号                    X(12)        */
                //strOrgImgSql.Append("'0'        		,	");  										 /*	2.	变更次数               	    Z(8)9        */
                //strOrgImgSql.Append("TO_CHAR(B.G_no,'FM999999990')           		,	");  			 /*	3.	帐册料件序号                Z(8)9        */
                //strOrgImgSql.Append("B.Cop_g_no       		,	");  								     /*	4.	货号                        X(30)        */
                //strOrgImgSql.Append("B.Code_t_s       		,	");  								     /*	5.	商品编码及附加商品编码      X(16)        */
                //strOrgImgSql.Append("B.Class_mark     		,	");  								     /*	6.	归类标志                    X(1)         */
                //strOrgImgSql.Append("B.G_name         		,	");  								     /*	7.	商品名称                    X(50)        */
                //strOrgImgSql.Append("B.G_model        		,	");  								     /*	8.	商品规格型号                X(50)        */
                //strOrgImgSql.Append("NVL(B.Unit,'000')           		,	");  						 /*	9.	申报计量单位                9(3)         */
                //strOrgImgSql.Append("NVL(B.Unit_1,'000')         		,	");  						 /*	10.	法定计量单位                9(3)         */
                //strOrgImgSql.Append("NVL(B.Unit_2,'000')         		,	");  						 /*	11.	法定第二单位                9(3)         */
                //strOrgImgSql.Append("NVL(B.Country_code,'000')   		,	");  						 /*	12.	产销国(地区)                9(3)         */
                //strOrgImgSql.Append("B.Source_mark    		,	");  								     /*	13.	来源标志                    X(1)         */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "')      		,	");  /*	14.	企业申报单价                Z(12)9.9(5)  */
                //strOrgImgSql.Append("NVL(B.Curr,'000')           		,	");  						 /*	15.	币制                        9(3)         */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Dec_price_rmb,0),'" + N13_5Format + "')  		,	");  /*	16.	申报单价人民币              Z(12)9.9(5)  */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Factor_1,0),'" + N9_9Format + "')       		,	");      /*	17.	法定计量单位比例因子        Z(8)9.9(9)   */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Factor_2,0),'" + N9_9Format + "')       		,	");      /*	18.	第二法定计量单位比例因子    Z(8)9.9(9)   */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Factor_wt,0),'" + N9_9Format + "')      		,	");      /*	19.	重量比例因子                Z(8)9.9(9)   */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Factor_rate,0),'FM9990.00000')    		,	");  	 /*	20.	比例因子浮动比率            Z(3)9.9(5)   */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "')            		,	");  /*	21.	申报进口数量                Z(12)9.9(5)  */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.Max_qty,0),'" + N13_5Format + "')        		,	");  /*	22.	批准最大余量                Z(12)9.9(5)  */
                //strOrgImgSql.Append("TO_CHAR(NVL(B.First_qty,0),'" + N13_5Format + "')      		,	");  /*	23.	初始库存数量                Z(12)9.9(5)  */
                //strOrgImgSql.Append("B.I_e_type           	,	");  								     /*	24.	进/出口方式                 X(1)         */
                //strOrgImgSql.Append("B.Use_type           	,	");  								     /*	25.	用途代码                    Z(8)9        */
                //strOrgImgSql.Append("B.Note_1             	,	");  								     /*	26.	备用标志1                   X(1)         */
                //strOrgImgSql.Append("B.Note_2             	,	");  								     /*	27.	备用标志2                   X(1)         */
                //strOrgImgSql.Append("B.Note               	,	");  								     /*	28.	备注                        X(10)        */
                //strOrgImgSql.Append("B.G_Eng_Name				,	");  								 /*	30	英文名称    			    X(50)        */
                //strOrgImgSql.Append("B.G_Eng_Model	        ,	");  								     /*	31	英文规格型号			    X(50)        */
                //strOrgImgSql.Append("B.Class_Note	       		,	");  								 /*	32	归类说明    			    X(2000)      */
                //strOrgImgSql.Append("B.Modify_mark        	    ");  								     /*	33	修改标志                    X(1)         */
                //strOrgImgSql.Append("from                       	");
                //strOrgImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_IMG B WHERE ");
                //strOrgImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                //strOrgImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strOrgImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strOrgImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strOrgImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strOrgImgSql.Append(" ORDER BY B.COP_G_NO");
                #endregion

                strOrgImgSql.Append("select					");
                strOrgImgSql.Append("B.COP_G_NO COP_G_NO                ,");                //<COP_G_NO>货号</COP_G_NO> 
                strOrgImgSql.Append("B.G_NO G_NO        		        ,");                //<G_NO>归并后序号</G_NO> 
                strOrgImgSql.Append("substr(B.CODE_T_S,0,9) CODE_T    	,");          //<CODE_T>商品编码</CODE_T>                    
                strOrgImgSql.Append("substr(B.CODE_T_S,9) CODE_S      	,");          //<CODE_S>附加编码</CODE_S>                    
                strOrgImgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                strOrgImgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>商品规格型号</G_MODEL> 
                strOrgImgSql.Append("B.ENT_UNIT ENT_UNIT         		    	,");          //<ENT_UNIT>自编计量单位</ENT_UNIT>    
                strOrgImgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                strOrgImgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                strOrgImgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                strOrgImgSql.Append("B.COUNTRY_CODE COUNTRY_CODE 	,");          //<COUNTRY_CODE>产销国(地区)</COUNTRY_CODE> 
                strOrgImgSql.Append("B.DEC_PRICE DEC_PRICE         	,");          //<DEC_PRICE>企业申报单价</DEC_PRICE>          
                strOrgImgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                strOrgImgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>
                strOrgImgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strOrgImgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strOrgImgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>   
                strOrgImgSql.Append("B.UNIT_RATIO UNIT_RATIO     ,");          //<UNIT_RATIO>自编计量单位比例</UNIT_RATIO> 
                strOrgImgSql.Append("B.QTY QTY         		        ,");          //<QTY>申报进口数量</QTY>                    
                strOrgImgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                strOrgImgSql.Append("B.FIRST_QTY FIRST_QTY         	,");          //<FIRST_QTY>初始库存数量</FIRST_QTY>                     
                strOrgImgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                strOrgImgSql.Append("B.NOTE_2 NOTE_2         		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                strOrgImgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>        
                strOrgImgSql.Append("B.MODIFY_MARK MODIFY_MARK        ,");                 //<MODIFY_MARK>修改标志</MODIFY_MARK> 
                strOrgImgSql.Append("B.DUTY_MODE         	           ,");           //<DUTY_MODE>征免方式</DUTY_MODE>        
                strOrgImgSql.Append("B.G_ENG_NAME				,	");  				//<G_ENG_NAME>商品英文名称</G_ENG_NAME>    
                strOrgImgSql.Append("B.G_ENG_MODEL	        ,	");  				//<G_ENG_MODEL>英文规格型号</G_ENG_MODEL>    
                strOrgImgSql.Append("B.CLASS_NOTE	       		");  					//<CLASS_NOTE>归类说明</CLASS_NOTE>    
                strOrgImgSql.Append("from                   ");
                strOrgImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_IMG B WHERE ");
                strOrgImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strOrgImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strOrgImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strOrgImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strOrgImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strOrgImgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strOrgImgSql.ToString(), QryExp, asaConn, out objRtnRult[3]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系归并前成品
                 */
                StringBuilder strOrgExgSql = new StringBuilder();

                #region 原归并关系归并前成品查询语句 comment by ccx 2010-5-18
                //strOrgExgSql.Append("select						");
                //strOrgExgSql.Append("A.Ems_no         		,	");  								     /*	1.	帐册编号                    X(12)        */
                //strOrgExgSql.Append("'0'        		,	");  										 /*	2.	变更次数               	    Z(8)9        */
                //strOrgExgSql.Append("TO_CHAR(B.G_no,'FM999999990')           		,	");  			 /*	3.	帐册料件序号                Z(8)9        */
                //strOrgExgSql.Append("B.Cop_g_no       		,	");  								     /*	4.	货号                        X(30)        */
                //strOrgExgSql.Append("B.Code_t_s       		,	");  								     /*	5.	商品编码及附加商品编码      X(16)        */
                //strOrgExgSql.Append("B.Class_mark     		,	");  								     /*	6.	归类标志                    X(1)         */
                //strOrgExgSql.Append("B.G_name         		,	");  								     /*	7.	商品名称                    X(50)        */
                //strOrgExgSql.Append("B.G_model        		,	");  								     /*	8.	商品规格型号                X(50)        */
                //strOrgExgSql.Append("NVL(B.Unit,'000')           		,	");  						 /*	9.	申报计量单位                9(3)         */
                //strOrgExgSql.Append("NVL(B.Unit_1,'000')         		,	");  						 /*	10.	法定计量单位                9(3)         */
                //strOrgExgSql.Append("NVL(B.Unit_2,'000')         		,	");  						 /*	11.	法定第二单位                9(3)         */
                //strOrgExgSql.Append("NVL(B.Country_code,'000')   		,	");  						 /*	12.	产销国(地区)                9(3)         */
                //strOrgExgSql.Append("B.Source_mark    		,	");  								     /*	13.	来源标志                    X(1)         */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "')      		,	");  /*	14.	企业申报单价                Z(12)9.9(5)  */
                //strOrgExgSql.Append("NVL(B.Curr,'000')           		,	");  						 /*	15.	币制                        9(3)         */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Dec_price_rmb,0),'" + N13_5Format + "')  		,	");  /*	16.	申报单价人民币              Z(12)9.9(5)  */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Factor_1,0),'" + N9_9Format + "')       		,	");      /*	17.	法定计量单位比例因子        Z(8)9.9(9)   */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Factor_2,0),'" + N9_9Format + "')       		,	");      /*	18.	第二法定计量单位比例因子    Z(8)9.9(9)   */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Factor_wt,0),'" + N9_9Format + "')      		,	");      /*	19.	重量比例因子                Z(8)9.9(9)   */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Factor_rate,0),'FM9990.00000')    		,	");  	 /*	20.	比例因子浮动比率            Z(3)9.9(5)   */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "')            		,	");  /*	21.	申报进口数量                Z(12)9.9(5)  */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.Max_qty,0),'" + N13_5Format + "')        		,	");  /*	22.	批准最大余量                Z(12)9.9(5)  */
                //strOrgExgSql.Append("TO_CHAR(NVL(B.First_qty,0),'" + N13_5Format + "')      		,	");  /*	23.	初始库存数量                Z(12)9.9(5)  */
                //strOrgExgSql.Append("B.I_e_type           	,	");  								     /*	24.	进/出口方式                 X(1)         */
                //strOrgExgSql.Append("B.Use_type           	,	");  								     /*	25.	用途代码                    Z(8)9        */
                //strOrgExgSql.Append("B.Note_1             	,	");  								     /*	26.	备用标志1                   X(1)         */
                //strOrgExgSql.Append("B.Note_2             	,	");  								     /*	27.	备用标志2                   X(1)         */
                //strOrgExgSql.Append("B.Note               	,	");  								     /*	28.	备注                        X(10)        */
                //strOrgExgSql.Append("B.G_Eng_Name				,	");  								 /*	30	英文名称    			    X(50)        */
                //strOrgExgSql.Append("B.G_Eng_Model	        ,	");  								     /*	31	英文规格型号			    X(50)        */
                //strOrgExgSql.Append("B.Class_Note	       		,	");  								 /*	32	归类说明    			    X(2000)      */
                //strOrgExgSql.Append("B.Modify_mark        	    ");  								     /*	33	修改标志                    X(1)         */
                //strOrgExgSql.Append("from                       	");
                //strOrgExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_EXG B  WHERE ");
                //strOrgExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                //strOrgExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strOrgExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strOrgExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strOrgExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strOrgExgSql.Append(" ORDER BY B.COP_G_NO");
                #endregion

                strOrgExgSql.Append("select					");
                strOrgExgSql.Append("B.COP_G_NO COP_G_NO                ,");                //<COP_G_NO>货号</COP_G_NO> 
                strOrgExgSql.Append("B.G_NO G_NO        		        ,");                //<G_NO>归并后序号</G_NO> 
                strOrgExgSql.Append("substr(B.CODE_T_S,0,9) CODE_T    	,");          //<CODE_T>商品编码</CODE_T>                    
                strOrgExgSql.Append("substr(B.CODE_T_S,9) CODE_S      	,");          //<CODE_S>附加编码</CODE_S>                    
                strOrgExgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                strOrgExgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>商品规格型号</G_MODEL> 
                strOrgExgSql.Append("B.ENT_UNIT ENT_UNIT         		    	,");          //<ENT_UNIT>自编计量单位</ENT_UNIT>    
                strOrgExgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                strOrgExgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                strOrgExgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                strOrgExgSql.Append("B.COUNTRY_CODE COUNTRY_CODE 	,");          //<COUNTRY_CODE>产销国(地区)</COUNTRY_CODE> 
                strOrgExgSql.Append("B.DEC_PRICE DEC_PRICE         	,");          //<DEC_PRICE>企业申报单价</DEC_PRICE>          
                strOrgExgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                strOrgExgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>
                strOrgExgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strOrgExgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strOrgExgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>   
                strOrgExgSql.Append("B.UNIT_RATIO UNIT_RATIO     ,");          //<UNIT_RATIO>自编计量单位比例</UNIT_RATIO> 
                strOrgExgSql.Append("B.QTY QTY         		        ,");          //<QTY>申报进口数量</QTY>                    
                strOrgExgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                strOrgExgSql.Append("B.FIRST_QTY FIRST_QTY         	,");          //<FIRST_QTY>初始库存数量</FIRST_QTY>                     
                strOrgExgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                strOrgExgSql.Append("'1' NOTE_2           		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                strOrgExgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>        
                strOrgExgSql.Append("B.MODIFY_MARK MODIFY_MARK        ,");                 //<MODIFY_MARK>修改标志</MODIFY_MARK> 
                strOrgExgSql.Append("B.DUTY_MODE         	           ,");           //<DUTY_MODE>征免方式</DUTY_MODE>        
                strOrgExgSql.Append("B.G_ENG_NAME				,	");  				//<G_ENG_NAME>商品英文名称</G_ENG_NAME>    
                strOrgExgSql.Append("B.G_ENG_MODEL	        ,	");  				//<G_ENG_MODEL>英文规格型号</G_ENG_MODEL>    
                strOrgExgSql.Append("B.CLASS_NOTE	       		");  					//<CLASS_NOTE>归类说明</CLASS_NOTE>    							    
                strOrgExgSql.Append("from                   ");
                strOrgExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_EXG B  WHERE ");
                strOrgExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strOrgExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strOrgExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strOrgExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strOrgExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strOrgExgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strOrgExgSql.ToString(), QryExp, asaConn, out objRtnRult[4]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                

                

                /*
                 * 归并关系BOM
                 */
                StringBuilder strBomSql = new StringBuilder();

                #region 原归并关系BOM查询语句 comment by ccx 2010-5-18
                //strBomSql.Append("select				");
                //strBomSql.Append("A.Ems_No         ,	");						/*1.	 帐册编号    	X(12)      	*/
                //strBomSql.Append("'000000000',	");								/*2.	 变更次数    	Z(8)9      	*/
                //strBomSql.Append("B.COP_EXG_NO,");								/*3.	 成品货号    	X(30)      	*/
                //strBomSql.Append("B.Begin_Date,");								/*4.	 开始有效日期	Z(8)       	*/
                //strBomSql.Append("B.End_Date,");								/*5.	 结束有效日期	Z(8)       	*/
                //strBomSql.Append("B.COP_IMG_NO,");								/*6.	 料件货号    	X(30)      	*/
                //strBomSql.Append("TO_CHAR(NVL(B.Dec_cm,0),'" + N9_9Format + "'),");  /*7.	 单耗        	Z(8)9.9(9) 	*/
                //strBomSql.Append("TO_CHAR(NVL(B.Dec_dm,0),'FM9990.000'),");		/*8.	 损耗        	Z(3)9.9(5) 	*/
                //strBomSql.Append("TO_CHAR(NVL(B.Other_cm,0),'" + N9_9Format + "'),");   /*9.	 其它单耗    	Z(8)9.9(9) 	*/
                //strBomSql.Append("TO_CHAR(NVL(B.Other_dm,0),'FM9990.000'),");   /*10	 其它损耗    	Z(3)9.9(5) 	*/
                //strBomSql.Append("B.Cm_mark       	,	");						/*11	 单耗标志    	X(1)       	*/
                //strBomSql.Append("B.Product_mark  	,	");						/*12	 加工流程标志	X(10)      	*/
                //strBomSql.Append("B.Product_type  	,	");						/*13	 加工性质    	X(1)       	*/
                //strBomSql.Append("B.Modify_mark   	,	");						/*14	 修改标志    	X(1)       	*/
                //strBomSql.Append("' '				,	");						/*15	 布控标志    	X(1)       	*/
                //strBomSql.Append("B.Note          	    ");						/*16	 备注        	X(10)      	*/
                //strBomSql.Append("from                 ");
                //strBomSql.Append("PRE_EMS3_HEAD A, PRE_EMS3_ORG_BOM B WHERE ");
                //strBomSql.Append(" B.MODIFY_MARK!='0' AND ");
                //strBomSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strBomSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strBomSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strBomSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strBomSql.Append(" ORDER BY B.COP_EXG_NO,B.BEGIN_DATE,B.COP_IMG_NO");
                #endregion

                strBomSql.Append("select				");
                strBomSql.Append("B.COP_EXG_NO COP_EXG_NO          	    	,");	  //<COP_EXG_NO>成品货号</COP_EXG_NO>   
                strBomSql.Append("B.COP_IMG_NO COP_IMG_NO          	    	,");	  //<COP_IMG_NO>料件货号</COP_IMG_NO> 					
                strBomSql.Append("B.BEGIN_DATE BEGIN_DATE      ,");	  //<EXG_VERSION_DATE>成品版本日期</EXG_VERSION_DATE> 					                        					
                strBomSql.Append("B.DEC_CM DEC_CM          	    			,");	  //<DEC_CM>净耗</DEC_CM>                         					
                strBomSql.Append("B.DEC_DM DEC_DM          	    			,");	  //<DEC_DM>损耗率</DEC_DM>                        					
                strBomSql.Append("B.MODIFY_MARK MODIFY_MARK          	    ,");	  //<MODIFY_MARK>修改标志</MODIFY_MARK>             					
                strBomSql.Append("CONVERT(VARCHAR,B.END_DATE,112) CON_VERSION          	    ,");	  //<EXG_VERSION>成品版本号</EXG_VERSION>            					
                //账册类型为电子账册时，此处必填归并后单耗版本（8位数值型）；电子手册时可空
                strBomSql.Append("CONVERT(VARCHAR,B.END_DATE,112) NOTE          	    				");	  //<NOTE>备注</NOTE>                             					
                strBomSql.Append("from                 ");
                strBomSql.Append("PRE_EMS3_HEAD A, PRE_EMS3_ORG_BOM B WHERE ");
                strBomSql.Append(" B.MODIFY_MARK!='0' AND ");
                strBomSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strBomSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strBomSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strBomSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strBomSql.Append(" ORDER BY B.COP_EXG_NO,B.BEGIN_DATE,B.COP_IMG_NO");

                bErrCode = Ems3Data.readTableByAdapter(strBomSql.ToString(), QryExp, asaConn, out objRtnRult[5]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                //归并前数据报文中的归并前料件
                StringBuilder strMrOrgImgSql = new StringBuilder();
                strMrOrgImgSql.Append("select					");
                strMrOrgImgSql.Append("B.G_NO G_NO        		        	,");      //<G_NO>料件序号</G_NO>                                  
                strMrOrgImgSql.Append("B.COP_G_NO COP_G_NO              	,");      //<COP_G_NO>料件货号</COP_G_NO>                         
                strMrOrgImgSql.Append("substr(B.CODE_T_S,0,9) CODE_T       	,");      //<CODE_T>商品编码</CODE_T>                              
                strMrOrgImgSql.Append("substr(B.CODE_T_S,9) CODE_S        	,");      //<CODE_S>附加编码</CODE_S>                              
                strMrOrgImgSql.Append("B.G_NAME G_NAME        		    	,");      //<G_NAME>商品名称</G_NAME>                              
                strMrOrgImgSql.Append("B.G_MODEL G_MODEL        			,");      //<G_MODEL>商品规格型号</G_MODEL>                          
                strMrOrgImgSql.Append("B.UNIT UNIT        		        	,");      //<UNIT>申报计量单位</UNIT>                                
                strMrOrgImgSql.Append("B.UNIT_1 UNIT_1        		    	,");      //<UNIT_1>法定计量单位</UNIT_1>                            
                strMrOrgImgSql.Append("B.UNIT_2 UNIT_2        		    	,");      //<UNIT_2>法定第二单位</UNIT_2>                            
                strMrOrgImgSql.Append("B.COUNTRY_CODE ORIGIN_COUNTRY    	,");      //<ORIGIN_COUNTRY>产销国(地区)</ORIGIN_COUNTRY>           
                strMrOrgImgSql.Append("B.DEC_PRICE UNIT_PRICE        		,");      //<UNIT_PRICE>企业申报单价</UNIT_PRICE>                    
                strMrOrgImgSql.Append("B.CURR CURR        		        	,");      //<CURR>币制</CURR>                                    
                strMrOrgImgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB        ,");      //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>           
                strMrOrgImgSql.Append("B.FACTOR_1 FACTOR_1        		    ,");      //<FACTOR_1>法定计量单位比例因子</FACTOR_1>                  
                strMrOrgImgSql.Append("B.FACTOR_2 FACTOR_2        		    ,");      //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>                
                strMrOrgImgSql.Append("B.FACTOR_WT FACTOR_WT        		,");      //<FACTOR_WT>重量比例因子</FACTOR_WT>                    
                strMrOrgImgSql.Append("B.FACTOR_RATE FACTOR_RATE        	,");      //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>              
                strMrOrgImgSql.Append("B.QTY I_QTY        		        	,");      //<I_QTY>申报进口数量</I_QTY>                            
                strMrOrgImgSql.Append("B.MAX_QTY MAX_QTY        		    ,");      //<MAX_QTY>批准最大余量</MAX_QTY>                        
                strMrOrgImgSql.Append("B.FIRST_QTY ORIGIN_QTY        	    ,");      //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>                  
                strMrOrgImgSql.Append("B.USE_TYPE USE_TYPE        		    ,");      //<USE_TYPE>用途代码</USE_TYPE>                        
                strMrOrgImgSql.Append("B.NOTE_1 NOTE_1        		        ,");      //<NOTE_1>备用标志1</NOTE_1>                           
                strMrOrgImgSql.Append("B.NOTE_2 NOTE_2        		        ,");      //<NOTE_2>备用标志2</NOTE_2>                           
                strMrOrgImgSql.Append("B.NOTE NOTE        		        	,");      //<NOTE>备注</NOTE>                                  
                strMrOrgImgSql.Append("B.MODIFY_MARK MODIFY_MARK        	,");      //<MODIFY_MARK>修改标志</MODIFY_MARK>                  								    
                strMrOrgImgSql.Append("'' APPR_AMT        		    		,");      //<APPR_AMT>总价</APPR_AMT>                         
                strMrOrgImgSql.Append("B.G_ENG_NAME G_ENAME        		    ,");      //<G_ENAME>英文名称</G_ENAME>                         
                strMrOrgImgSql.Append("B.G_ENG_MODEL G_EMODEL        		,");      //<G_EMODEL>英文规格型号</G_EMODEL>                     
                strMrOrgImgSql.Append("B.CLASS_NOTE CLASS_NOTE        		,");      //<CLASS_NOTE>归类说明</CLASS_NOTE>                   
                strMrOrgImgSql.Append("B.ENT_UNIT COP_UNIT        		    ,");      //<COP_UNIT>企业自编计量单位</COP_UNIT>                   
                strMrOrgImgSql.Append("B.UNIT_RATIO COP_FACTOR        	    ,");      //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>           
                strMrOrgImgSql.Append("B.DUTY_MODE DUTY_MODE        	    ");       //<DUTY_MODE>征免方式</DUTY_MODE>                    
                strMrOrgImgSql.Append("from                   ");
                strMrOrgImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_IMG B WHERE ");
                strMrOrgImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strMrOrgImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strMrOrgImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strMrOrgImgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strMrOrgImgSql.ToString(), QryExp, asaConn, out objRtnRult[6]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[6] != null)
                {
                    objRtnRult[6].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[6].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[6].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[6].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[6].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END

                //归并前数据报文中的归并前成品
                StringBuilder strMrOrgExgSql = new StringBuilder();
                strMrOrgExgSql.Append("select					");
                strMrOrgExgSql.Append("B.G_NO G_NO        		        	,");      //<G_NO>成品序号</G_NO>                                  
                strMrOrgExgSql.Append("B.COP_G_NO COP_G_NO              	,");      //<COP_G_NO>成品货号</COP_G_NO>                         
                strMrOrgExgSql.Append("substr(B.CODE_T_S,0,9) CODE_T       	,");      //<CODE_T>商品编码</CODE_T>                              
                strMrOrgExgSql.Append("substr(B.CODE_T_S,9) CODE_S        	,");      //<CODE_S>附加编码</CODE_S>                              
                strMrOrgExgSql.Append("B.G_NAME G_NAME        		    	,");      //<G_NAME>商品名称</G_NAME>                              
                strMrOrgExgSql.Append("B.G_MODEL G_MODEL        			,");      //<G_MODEL>商品规格型号</G_MODEL>                          
                strMrOrgExgSql.Append("B.UNIT UNIT        		        	,");      //<UNIT>申报计量单位</UNIT>                                
                strMrOrgExgSql.Append("B.UNIT_1 UNIT_1        		    	,");      //<UNIT_1>法定计量单位</UNIT_1>                            
                strMrOrgExgSql.Append("B.UNIT_2 UNIT_2        		    	,");      //<UNIT_2>法定第二单位</UNIT_2>                            
                strMrOrgExgSql.Append("B.COUNTRY_CODE ORIGIN_COUNTRY    	,");      //<ORIGIN_COUNTRY>产销国(地区)</ORIGIN_COUNTRY>           
                strMrOrgExgSql.Append("B.DEC_PRICE UNIT_PRICE        		,");      //<UNIT_PRICE>企业申报单价</UNIT_PRICE>                    
                strMrOrgExgSql.Append("B.CURR CURR        		        	,");      //<CURR>币制</CURR>                                    
                strMrOrgExgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB        ,");      //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>           
                strMrOrgExgSql.Append("B.FACTOR_1 FACTOR_1        		    ,");      //<FACTOR_1>法定计量单位比例因子</FACTOR_1>                  
                strMrOrgExgSql.Append("B.FACTOR_2 FACTOR_2        		    ,");      //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>                
                strMrOrgExgSql.Append("B.FACTOR_WT FACTOR_WT        		,");      //<FACTOR_WT>重量比例因子</FACTOR_WT>                    
                strMrOrgExgSql.Append("B.FACTOR_RATE FACTOR_RATE        	,");      //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>              
                strMrOrgExgSql.Append("B.QTY I_QTY        		        	,");      //<I_QTY>申报进口数量</I_QTY>                            
                strMrOrgExgSql.Append("B.MAX_QTY MAX_QTY        		    ,");      //<MAX_QTY>批准最大余量</MAX_QTY>                        
                strMrOrgExgSql.Append("B.FIRST_QTY ORIGIN_QTY        	    ,");      //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>                  
                strMrOrgExgSql.Append("B.USE_TYPE USE_TYPE        		    ,");      //<USE_TYPE>用途代码</USE_TYPE>                        
                strMrOrgExgSql.Append("B.NOTE_1 NOTE_1        		        ,");      //<NOTE_1>备用标志1</NOTE_1>                           
                strMrOrgExgSql.Append("B.NOTE_2 NOTE_2        		        ,");      //<NOTE_2>备用标志2</NOTE_2>                           
                strMrOrgExgSql.Append("B.NOTE NOTE        		        	,");      //<NOTE>备注</NOTE>                                  
                strMrOrgExgSql.Append("B.MODIFY_MARK MODIFY_MARK        	,");      //<MODIFY_MARK>修改标志</MODIFY_MARK>                  								    
                strMrOrgExgSql.Append("'' APPR_AMT        		    		,");      //<APPR_AMT>总价</APPR_AMT>                         
                strMrOrgExgSql.Append("B.G_ENG_NAME G_ENAME        		    ,");      //<G_ENAME>英文名称</G_ENAME>                         
                strMrOrgExgSql.Append("B.G_ENG_MODEL G_EMODEL        		,");      //<G_EMODEL>英文规格型号</G_EMODEL>                     
                strMrOrgExgSql.Append("B.CLASS_NOTE CLASS_NOTE        		,");      //<CLASS_NOTE>归类说明</CLASS_NOTE>                   
                strMrOrgExgSql.Append("B.ENT_UNIT COP_UNIT        		    ,");      //<COP_UNIT>企业自编计量单位</COP_UNIT>                   
                strMrOrgExgSql.Append("B.UNIT_RATIO COP_FACTOR        	    ,");      //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>           
                strMrOrgExgSql.Append("B.DUTY_MODE DUTY_MODE        	    ");       //<DUTY_MODE>征免方式</DUTY_MODE>                    
                strMrOrgExgSql.Append("from                   ");
                strMrOrgExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_EXG B WHERE ");
                strMrOrgExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strMrOrgExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strMrOrgExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strMrOrgExgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strMrOrgExgSql.ToString(), QryExp, asaConn, out objRtnRult[7]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[7] != null)
                {
                    objRtnRult[7].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[7].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[7].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[7].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[7].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END
                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                //   DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
                DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }
        #endregion

        private static readonly string strHeadSql = "select COP_EMS_NO,TRADE_CODE,TRADE_NAME,OWNER_CODE,OWNER_NAME,OWNER_CODE as DECLARE_CODE,OWNER_NAME as DECLARE_NAME,EMS_TYPE,DECLARE_TYPE,TRADE_MODE,CONVERT(VARCHAR,BEGIN_DATE,112) BEGIN_DATE,CONVERT(VARCHAR,END_DATE,112) END_DATE,EMS_APPR_NO,CONTR_NO,ICCARD_ID,CORR_EMS_NO,NOTE_1,NOTE_2,NOTE_AMOUNT,NOTE_QTY,CONVERT(VARCHAR,INPUT_DATE,112) INPUT_DATE,INPUT_ER,PRODUCT_RATIO,STORE_AREA,I_E_PORT,FOREIGN_CO_NAME,AGREE_NO,CUT_MODE,PAY_MODE,PRODUCE_TYPE,CONTR_OUT,STORE_VOL,NOTE FROM PRE_EMS3_HEAD WHERE COP_EMS_NO = {COP_EMS_NO} AND TRADE_CODE = {TRADE_CODE} ";
        private static readonly string strOrgExgSql = "select COP_G_NO,G_NO,SUBSTR(CODE_T_S,1,8) as CODE_T,SUBSTR(CODE_T_S,9,2) as CODE_S,G_NAME,G_MODEL,ENT_UNIT,UNIT,UNIT_1,UNIT_2,COUNTRY_CODE,DEC_PRICE,CURR,DEC_PRICE_RMB,FACTOR_1,FACTOR_2,FACTOR_WT,UNIT_RATIO,QTY,MAX_QTY,FIRST_QTY,NOTE_1,NOTE_2,NOTE,MODIFY_MARK,DUTY_MODE,G_ENG_NAME,G_ENG_MODEL,CLASS_NOTE from PRE_EMS3_ORG_EXG WHERE MODIFY_MARK!='0' AND COP_EMS_NO={COP_EMS_NO} AND TRADE_CODE={TRADE_CODE} ORDER BY COP_G_NO";
        private static readonly string strOrgImgSql = "select COP_G_NO,G_NO,SUBSTR(CODE_T_S,1,8) as CODE_T,SUBSTR(CODE_T_S,9,2) as CODE_S,G_NAME,G_MODEL,ENT_UNIT,UNIT,UNIT_1,UNIT_2,COUNTRY_CODE,DEC_PRICE,CURR,DEC_PRICE_RMB,FACTOR_1,FACTOR_2,FACTOR_WT,UNIT_RATIO,QTY,MAX_QTY,FIRST_QTY,NOTE_1,NOTE_2,NOTE,MODIFY_MARK,DUTY_MODE,G_ENG_NAME,G_ENG_MODEL,CLASS_NOTE from PRE_EMS3_ORG_IMG WHERE MODIFY_MARK!='0' AND COP_EMS_NO={COP_EMS_NO} AND TRADE_CODE={TRADE_CODE} ORDER BY COP_G_NO";
        private static readonly string strExgSql = "select G_NO,COP_G_NO,SUBSTR(CODE_T_S,1,8) as CODE_T,SUBSTR(CODE_T_S,9,2) as CODE_S,G_NAME,G_MODEL,UNIT,UNIT_1,UNIT_2,COUNTRY_CODE,DEC_PRICE,CURR,DEC_PRICE_RMB,FACTOR_1,FACTOR_2,FACTOR_WT,QTY,MAX_QTY,FIRST_QTY,NOTE_1,NOTE_2,NOTE,MODIFY_MARK,DUTY_MODE from PRE_EMS3_EXG WHERE MODIFY_MARK!='0' AND COP_EMS_NO={COP_EMS_NO} AND TRADE_CODE={TRADE_CODE} ORDER BY G_NO";
        private static readonly string strImgSql = "select G_NO,COP_G_NO,SUBSTR(CODE_T_S,1,8) as CODE_T,SUBSTR(CODE_T_S,9,2) as CODE_S,G_NAME,G_MODEL,UNIT,UNIT_1,UNIT_2,COUNTRY_CODE,DEC_PRICE,CURR,DEC_PRICE_RMB,FACTOR_1,FACTOR_2,FACTOR_WT,QTY,MAX_QTY,FIRST_QTY,NOTE_1,NOTE_2,NOTE,MODIFY_MARK,DUTY_MODE from PRE_EMS3_IMG WHERE MODIFY_MARK!='0' AND COP_EMS_NO={COP_EMS_NO} AND TRADE_CODE={TRADE_CODE} ORDER BY G_NO";
        private static readonly string strBomSql = "select COP_EXG_NO,COP_IMG_NO,BEGIN_DATE,DEC_CM,DEC_DM,MODIFY_MARK,'' as CON_VERSION,NOTE from PRE_EMS3_ORG_BOM WHERE MODIFY_MARK!='0' AND COP_EMS_NO={COP_EMS_NO} AND TRADE_CODE={TRADE_CODE} ORDER BY COP_EXG_NO,BEGIN_DATE,COP_IMG_NO";

        /// <summary>
        /// 归并关系新接口
        /// </summary>
        /// <param name="strSysFlg">系统标志H88、H2000</param>
        /// <param name="strMsgType">报文类型</param>
        /// <param name="strTradeCode">企业十位编码</param>
        /// <param name="strCopEmsNo">企业内部编号</param>
        /// <param name="strOtherPara">其他差数,如:中期和查的开始日期，报核的报核次数</param>
        /// <param name="objRtnRult">返回结果</param>
        /// <param name="strErrMsg">返回消息</param>
        /// <returns></returns>
        public static int getMrDataOfMsgForM(string strSysFlg,string strMsgType,string strTradeCode,string strCopEmsNo,string strOtherPara,ref DataTable[] objRtnRult,ref string strErrMsg)
        {
            /*
             * 归并关系表头
             */
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);

            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[6];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系归并后料件
                 */
                bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                /*
                 * 归并关系归并后成品
                 */
                bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系归并前料件
                 */
                bErrCode = Ems3Data.readTableByAdapter(strOrgImgSql.ToString(), QryExp, asaConn, out objRtnRult[3]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系归并前成品
                 */
                bErrCode = Ems3Data.readTableByAdapter(strOrgExgSql.ToString(), QryExp, asaConn, out objRtnRult[4]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 归并关系BOM
                 */
                bErrCode = Ems3Data.readTableByAdapter(strBomSql.ToString(), QryExp, asaConn, out objRtnRult[5]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }

        public static int getEmsDataOfMsg(//电子帐册
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {//错误信息
            /*
             * 电子帐册表头
             */
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);

            #region 原电子账册表头查询语句 comment by ccx 2010-5-19
            //strHeadSql.Append("select									");
            //strHeadSql.Append("Ems_no          				,			");         /*1.		帐册编号          X(12)       */
            //strHeadSql.Append("'000000000'     				,			");         /*2.		#变更次数         9(9)        */
            //strHeadSql.Append("Pre_ems_no      				,			");         /*3.		预申报帐册编号    X(12)       */
            //strHeadSql.Append("Cop_ems_no      				,			");         /*4.		企业内部编号      X(20)       */
            //strHeadSql.Append("Trade_code      				,			");         /*5.		经营单位代码      X(10)       */
            //strHeadSql.Append("Trade_name      				,			");         /*6.		经营单位名称      X(30)       */
            //strHeadSql.Append("House_no        				,			");         /*7.		仓库编号          X(12)       */
            //strHeadSql.Append("Owner_code      				,			");         /*8.		收货单位代码      X(10)       */
            //strHeadSql.Append("Owner_name      				,			");         /*9.		收货单位名称      X(30)       */
            //strHeadSql.Append("Owner_type      				,			");         /*10.		企业性质          X           */
            //strHeadSql.Append("Declare_er_type 				,			");         /*11.		申请单位类型      X(1)        */
            //strHeadSql.Append("Declare_code    				,			");         /*12.		申请单位代码      X(10)       */
            //strHeadSql.Append("Declare_name    				,			");         /*13.		申请单位名称      X(30)       */
            //strHeadSql.Append("SUBSTR(District_code,1,5)	,			");         /*14.		地区代码          Z(5)        */
            //strHeadSql.Append("Address         				,			");         /*15.		联系地址          X(30)       */
            //strHeadSql.Append("Phone           				,			");         /*16.		电话号码          X(20)       */
            //strHeadSql.Append("Ems_type        				,			");         /*17.		帐册类型          X(1)        */
            //strHeadSql.Append("Declare_type    				,			");         /*18.		申报类型          X(1)        */
            //strHeadSql.Append("Invest_mode     				,			");         /*19.		投资方式          X(1)        */
            //strHeadSql.Append("NVL(Trade_mode,'0000')		,			");         /*20.		贸易方式          9(4)        */
            //strHeadSql.Append("TO_CHAR(Begin_date,'YYYYMMDD'),			");         /*21.		开始有效期        Z(8)        */
            //strHeadSql.Append("TO_CHAR(End_date,'YYYYMMDD') ,			");         /*22.		结束有效期        Z(8)        */
            //strHeadSql.Append("TO_CHAR(NVL(Img_amount,0),'" + N13_5Format + "'),");     /*23.		进口总金额        Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_amount,0),'" + N13_5Format + "'),");		/*24		出口总金额		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Img_weight,0),'" + N13_5Format + "'),");		/*25		进口总重量		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_weight,0),'" + N13_5Format + "'),");		/*26		出口总重量		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Img_items,0),'FM999999990'),");			/*27		进口货物项数	  Z(8)9       */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_items,0),'FM999999990'),");			/*28		出口货物项数	  Z(8)9       */
            //strHeadSql.Append("Ems_appr_no     				,			");         /*29.		批准证编号        X(20)       */
            //strHeadSql.Append("license_no      				,			");         /*30.		许可证编号        X(20)       */
            //strHeadSql.Append("Last_ems_no     				,			");         /*31.		对应上本帐册号    X(12)       */
            //strHeadSql.Append("corr_ems_no     				,			");         /*32.		对应其它帐册号    X(12)       */
            //strHeadSql.Append("Contr_no        				,			");         /*33.		合同号            X(20)       */
            //strHeadSql.Append("Iccard_id       				,			");         /*34.		身份识别号        X(20)       */
            //strHeadSql.Append("Id_card_pwd     				,			");         /*35.		身份识别密码      X(20)       */
            //strHeadSql.Append("Note_1          				,			");         /*36.		备用标志1         X(10)       */
            //strHeadSql.Append("Note_2          				,			");         /*37.		备用标志2         X(10)       */
            //strHeadSql.Append("TO_CHAR(NVL(Invest_amount,0),'" + N13_5Format + "'),");  /*38		投资金额		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Note_amount,0),'" + N13_5Format + "'),");    /*39		备用金额		  Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(Note_qty,0),'" + N13_5Format + "'),");	    /*40		备用数量		  Z(12)9.9(5) */
            //strHeadSql.Append("Note            				,			");         /*41.		备注              X(50)       */
            //strHeadSql.Append("TO_CHAR(Input_date,'YYYYMMDD'),			");         /*42.		录入日期          Z(8)        */
            //strHeadSql.Append("TO_CHAR(NVL(Input_er,0),'FM0000'),		");         /*43.		录入员代号        9(4)        */
            //strHeadSql.Append("TO_CHAR(Declare_date,'YYYYMMDD'),			");         /*44.		申报日期          Z(8)        */
            //strHeadSql.Append("TO_CHAR(Declare_date,'HH24MMss'),		");         /*45.		申报时间          Z(8)        */
            //strHeadSql.Append("Ems_appr_mark   				,			");         /*46.		其它部门审批标志  X(10)       */
            //strHeadSql.Append("Ems_certify     				,			");         /*47.		其它单证标志      X(10)       */
            //strHeadSql.Append("TO_CHAR(NVL(Product_ratio,0),'" + N13_5Format + "'),");  /*48.		生产能力          Z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(STORE_VOL,0),'" + N13_5Format + "'),");      /*49.		仓库体积          z(12)9.9(5) */
            //strHeadSql.Append("TO_CHAR(NVL(STORE_AREA,0),'" + N13_5Format + "'),");     /*50.		仓库面积          z(12)9.9(5) */
            //strHeadSql.Append("I_E_PORT                     ,			");         /*51.		进出口岸          X(255)      */
            //strHeadSql.Append("FOREIGN_CO_NAME              ,			");         /*52.		外商公司          X(255)      */
            //strHeadSql.Append("AGREE_NO                     ,			");         /*53.		协议号            X(32)       */
            //strHeadSql.Append("CUT_MODE                     ,			");         /*54.		征免性质          X(4)        */
            //strHeadSql.Append("PAY_MODE                     ,			");         /*55.		保税方式          X(1)        */
            //strHeadSql.Append("PRODUCE_TYPE                 ,			");         /*56.		加工种类          X(2)        */
            //strHeadSql.Append("CONTR_OUT                    ,			");         /*57.		出口合同号        X(32)       */
            //strHeadSql.Append("Modify_mark                				");	        /*58.		修改标志		  X(1)        */
            //strHeadSql.Append("FROM PRE_EMS3_CUS_HEAD WHERE ");
            //strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            //strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
            #endregion

            strHeadSql.Append("select									");
            strHeadSql.Append("EMS_NO          				            ,"); 	  //<EMS_NO>电子手册编号</EMS_NO>             
            strHeadSql.Append("COP_EMS_NO          				        ,");      //<COP_EMS_NO>企业内部编号</COP_EMS_NO>     
            strHeadSql.Append("TRADE_CODE          				        ,");      //<TRADE_CODE>经营单位代码</TRADE_CODE>     
            strHeadSql.Append("TRADE_NAME          				        ,");      //<TRADE_NAME>经营单位名称</TRADE_NAME>     
            strHeadSql.Append("HOUSE_NO          				        ,");      //<HOUSE_NO>仓库编号</HOUSE_NO>           
            strHeadSql.Append("OWNER_CODE          				        ,");      //<OWNER_CODE>加工单位代码</OWNER_CODE>     
            strHeadSql.Append("OWNER_NAME          				        ,");      //<OWNER_NAME>加工单位名称</OWNER_NAME>     
            strHeadSql.Append("OWNER_TYPE          				        ,");      //<OWNER_TYPE>企业性质</OWNER_TYPE>       
            strHeadSql.Append("TRADE_CODE DECLARE_CODE          		,");      //<DECLARE_CODE>申请单位代码</DECLARE_CODE> 
            strHeadSql.Append("DECLARE_NAME          			        ,");      //<DECLARE_NAME>申请单位名称</DECLARE_NAME> 
            strHeadSql.Append("'0' DISTRICT_CODE          			    ,");      //<DISTRICT_CODE>地区代码</DISTRICT_CODE> 
            strHeadSql.Append("ADDRESS          				        ,");      //<ADDRESS>联系地址</ADDRESS>             
            strHeadSql.Append("PHONE          				            ,");      //<PHONE>电话号码</PHONE>                 
            strHeadSql.Append("EMS_TYPE          				        ,");      //<EMS_TYPE>帐册类型</EMS_TYPE>       
            strHeadSql.Append("INVEST_MODE          				        ,"); 	  //<INVEST_MODE>投资方式或台帐银行</INVEST_MODE> 
            strHeadSql.Append("TRADE_MODE          				            ,");      //<TRADE_MODE>贸易方式</TRADE_MODE>        
            strHeadSql.Append("CONVERT(VARCHAR,BEGIN_DATE,112) BEGIN_DATE   ,");      //<BEGIN_DATE>开始有效期</BEGIN_DATE>       
            strHeadSql.Append("CONVERT(VARCHAR,END_DATE,112) END_DATE       ,");      //<END_DATE>结束有效期</END_DATE>           
            strHeadSql.Append("IMG_AMOUNT          				            ,");      //<IMG_AMOUNT>进口总金额</IMG_AMOUNT>       
            strHeadSql.Append("EXG_AMOUNT          				            ,");      //<EXG_AMOUNT>出口总金额</EXG_AMOUNT>       
            strHeadSql.Append("IMG_WEIGHT          				            ,");      //<IMG_WEIGHT>进口总重量</IMG_WEIGHT>       
            strHeadSql.Append("EXG_WEIGHT          				            ,");      //<EXG_WEIGHT>出口总重量</EXG_WEIGHT>       
            strHeadSql.Append("IMG_ITEMS          				            ,");      //<IMG_ITEMS>进口货物项数</IMG_ITEMS>        
            strHeadSql.Append("EXG_ITEMS          				            ,");      //<EXG_ITEMS>出口货物项数</EXG_ITEMS>        
            strHeadSql.Append("EMS_APPR_NO          				        ,");      //<EMS_APPR_NO>批准文号</EMS_APPR_NO>      
            strHeadSql.Append("LICENSE_NO          				            ,");      //<LICENSE_NO>许可证号</LICENSE_NO>        
            strHeadSql.Append("LAST_EMS_NO          				        ,");      //<LAST_EMS_NO>对应上本帐册号</LAST_EMS_NO>   
            strHeadSql.Append("CORR_EMS_NO          				        ,");      //<CORR_EMS_NO>对应其它帐册号</CORR_EMS_NO>   
            strHeadSql.Append("CONTR_NO CONTR_IN          				    ,");      //<CONTR_IN>进口合同号</CONTR_IN>           
            strHeadSql.Append("NOTE_1          				                ,");      //<NOTE_1>备用标志1</NOTE_1>               
            strHeadSql.Append("NOTE_2          				                ,");      //<NOTE_2>备用标志2</NOTE_2>               
            strHeadSql.Append("INVEST_AMOUNT          				            ,");	  //<INVEST_AMOUNT>投资金额</INVEST_AMOUNT>      
            strHeadSql.Append("NOTE_AMOUNT          				            ,");      //<NOTE_AMOUNT>备用金额</NOTE_AMOUNT>          
            strHeadSql.Append("NOTE_QTY          				                ,");      //<NOTE_QTY>备用数量</NOTE_QTY>                
            strHeadSql.Append("NOTE          				                	,");      //<NOTE>备注</NOTE>                          
            strHeadSql.Append("CONVERT(VARCHAR,INPUT_DATE,112) INPUT_DATE       ,");      //<INPUT_DATE>录入日期</INPUT_DATE>            
            strHeadSql.Append("INPUT_ER          				                ,");      //<INPUT_ER>录入员代号</INPUT_ER>               
            strHeadSql.Append("CONVERT(VARCHAR,DECLARE_DATE,112) DECLARE_DATE	,");      //<DECLARE_DATE>申报日期</DECLARE_DATE>        
            strHeadSql.Append("'08301020' DECLARE_TIME          				        ,");      //<DECLARE_TIME>申报时间</DECLARE_TIME>        
            strHeadSql.Append("PRODUCT_RATIO          				            ,");      //<PRODUCT_RATIO>生产能力</PRODUCT_RATIO>      
            strHeadSql.Append("STORE_VOL          				                ,");      //<STORE_VOL>仓库体积</STORE_VOL>              
            strHeadSql.Append("STORE_AREA          				                ,");      //<STORE_AREA>仓库面积</STORE_AREA>            
            strHeadSql.Append("I_E_PORT          				                ,");      //<I_E_PORT>进出口岸</I_E_PORT>                
            strHeadSql.Append("FOREIGN_CO_NAME          				        ,");      //<FOREIGN_CO_NAME>外商公司</FOREIGN_CO_NAME>  
            strHeadSql.Append("AGREE_NO          				                ,");      //<AGREE_NO>协议号</AGREE_NO>                 
            strHeadSql.Append("CUT_MODE          				                ,");      //<CUT_MODE>征免性质</CUT_MODE>                
            strHeadSql.Append("PAY_MODE          				                ,");      //<PAY_MODE>保税方式</PAY_MODE>                
            strHeadSql.Append("PRODUCE_TYPE          				            ,");      //<PRODUCE_TYPE>加工种类</PRODUCE_TYPE>        
            strHeadSql.Append("CONTR_OUT          				                ,");      //<CONTR_OUT>出口合同号</CONTR_OUT>             
            strHeadSql.Append("'0' APPR_IMG_AMT          				        ,");      //<APPR_IMG_AMT>备案进口总值</APPR_IMG_AMT>      
            strHeadSql.Append("'0' APPR_EXG_AMT          				        ,");      //<APPR_EXG_AMT>备案出口总值</APPR_EXG_AMT>      
            strHeadSql.Append("'' FOREIGN_MGR          				 	,");	  //<FOREIGN_MGR>外商经理人</FOREIGN_MGR>      
            strHeadSql.Append("'' TRANS_MODE          				    ,");      //<TRANS_MODE>成交方式</TRANS_MODE>         
            strHeadSql.Append("'' TRADE_COUNTRY          				,");      //<TRADE_COUNTRY>起抵地</TRADE_COUNTRY>    
            strHeadSql.Append("'0' EQUIP_MODE          				    ,");      //<EQUIP_MODE>单耗申报环节</EQUIP_MODE>       
            strHeadSql.Append("'' IN_RATIO          				    ,");      //<IN_RATIO>内销比率</IN_RATIO>             
            strHeadSql.Append("'0' EX_CURR          				        ,");      //<EX_CURR>出口币制</EX_CURR>               
            strHeadSql.Append("'0' IM_CURR          				        ,");      //<IM_CURR>进口币制</IM_CURR>               
            strHeadSql.Append("isnull(MODIFY_MARK,'0') MODIFY_MARK      ,");      //<MODIFY_MARK>修改标志</MODIFY_MARK>       
            strHeadSql.Append("'0' MASTER_CUSTOMS          				,");      //<MASTER_CUSTOMS>主管海关</MASTER_CUSTOMS> 
            strHeadSql.Append("'0' MASTER_FTC          				    ,");      //<MASTER_FTC>主管外经贸部门</MASTER_FTC>      
            strHeadSql.Append("'0' MANAGE_OBJECT          				,");      //<MANAGE_OBJECT>管理对象</MANAGE_OBJECT>   
            strHeadSql.Append("'' COP_ENT_NO          				    ,");      //<COP_ENT_NO>企业内部物料编号</COP_ENT_NO>       
            strHeadSql.Append("'' LIMIT_FLAG          				    ");      //<LIMIT_FLAG>限制类标志</ LIMIT_FLAG >        
            strHeadSql.Append("FROM PRE_EMS3_CUS_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");

            //  OracleConnection asaConn = DataManager.GetBizConn();
            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[4];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[0] != null)
                {
                    objRtnRult[0].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[0].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[0].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[0].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[0].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END
                //处理CONTR_IN——“进口合同号”字段，如果为空，赋值0，否则报文导入程序校验不通过
                if (objRtnRult[0] != null && objRtnRult[0].Rows.Count > 0)
                {
                    for (int row = 0; row < objRtnRult[0].Rows.Count; row++)
                    {
                        if (objRtnRult[0].Rows[row]["CONTR_IN"] == null || objRtnRult[0].Rows[row]["CONTR_IN"].ToString() == string.Empty)
                        {
                            objRtnRult[0].Rows[row]["CONTR_IN"] = "0";
                        }
                    }
                }

                /*
                 * 电子帐册成品
                 */
                StringBuilder strExgSql = new StringBuilder();

                #region 原电子账册成品查询语句 comment by ccx 2010-5-19
                //strExgSql.Append("select						");
                //strExgSql.Append("A.Ems_no         		,	");  								     /*	1.	帐册编号                  X(12)        */
                //strExgSql.Append("'0'        		,	");  										 /*	2.	变更次数               	  Z(8)9        */
                //strExgSql.Append("TO_CHAR(B.G_no,'FM999999990')           		,	");  			 /*	3.	帐册料件序号              Z(8)9        */
                //strExgSql.Append("B.Cop_g_no       		,	");  								     /*	4.	货号                      X(30)        */
                //strExgSql.Append("B.Code_t_s       		,	");  								     /*	5.	商品编码及附加商品编码    X(16)        */
                //strExgSql.Append("B.Class_mark     		,	");  								     /*	6.	归类标志                  X(1)         */
                //strExgSql.Append("B.G_name         		,	");  								     /*	7.	商品名称                  X(50)        */
                //strExgSql.Append("B.G_model        		,	");  								     /*	8.	商品规格型号              X(50)        */
                //strExgSql.Append("NVL(B.Unit,'000')           		,	");  						 /*	9.	申报计量单位              9(3)         */
                //strExgSql.Append("NVL(B.Unit_1,'000')         		,	");  						 /*	10.	法定计量单位              9(3)         */
                //strExgSql.Append("NVL(B.Unit_2,'000')         		,	");  						 /*	11.	法定第二单位              9(3)         */
                //strExgSql.Append("NVL(B.Country_code,'000')   		,	");  						 /*	12.	产销国(地区)              9(3)         */
                //strExgSql.Append("B.Source_mark    		,	");  								     /*	13.	来源标志                  X(1)         */
                //strExgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "')      		,	");  /*	14.	企业申报单价              Z(12)9.9(5)  */
                //strExgSql.Append("NVL(B.Curr,'000')           		,	");  						 /*	15.	币制                      9(3)         */
                //strExgSql.Append("TO_CHAR(NVL(B.Dec_price_rmb,0),'" + N13_5Format + "')  		,	");  /*	16.	申报单价人民币            Z(12)9.9(5)  */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_1,0),'" + N9_9Format + "')       		,	");  /*	17.	法定计量单位比例因子      Z(8)9.9(9)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_2,0),'" + N9_9Format + "')       		,	");  /*	18.	第二法定计量单位比例因子  Z(8)9.9(9)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_wt,0),'" + N9_9Format + "')      		,	");  /*	19.	重量比例因子              Z(8)9.9(9)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Factor_rate,0),'FM9990.00000')    		,	");  	 /*	20.	比例因子浮动比率          Z(3)9.9(5)   */
                //strExgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "')            		,	");  /*	21.	申报进口数量              Z(12)9.9(5)  */
                //strExgSql.Append("TO_CHAR(NVL(B.Max_qty,0),'" + N13_5Format + "')        		,	");  /*	22.	批准最大余量              Z(12)9.9(5)  */
                //strExgSql.Append("TO_CHAR(NVL(B.First_qty,0),'" + N13_5Format + "')      		,	");  /*	23.	初始库存数量              Z(12)9.9(5)  */
                //strExgSql.Append("B.I_e_type           	,	");  								     /*	24.	进/出口方式               X(1)         */
                //strExgSql.Append("B.Use_type           	,	");  								     /*	25.	用途代码                  Z(8)9        */
                //strExgSql.Append("B.Note_1             	,	");  								     /*	26.	备用标志1                 X(1)         */
                //strExgSql.Append("B.Note_2             	,	");  								     /*	27.	备用标志2                 X(1)         */
                //strExgSql.Append("B.Note               	,	");  								     /*	28.	备注                      X(10)        */
                //strExgSql.Append("B.Modify_mark        	    ");  								     /*	29	修改标志                  X(1)         */
                //strExgSql.Append("from                       	");
                //strExgSql.Append("PRE_EMS3_CUS_HEAD A,PRE_EMS3_CUS_EXG B WHERE ");
                //strExgSql.Append(" B.MODIFY_MARK!='0' ");
                //strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strExgSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                //strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strExgSql.Append(" ORDER BY B.G_NO");
                #endregion

  
                strExgSql.Append("select						");
                strExgSql.Append("B.G_NO G_NO        		    	,");		  //<G_NO>料件序号</G_NO>                        
                strExgSql.Append("B.COP_G_NO COP_G_NO        		,");          //<COP_G_NO>货号</COP_G_NO>                  
                strExgSql.Append("substr(B.CODE_T_S,0,9) CODE_T    	,");          //<CODE_T>商品编号</CODE_T>                    
                strExgSql.Append("substr(B.CODE_T_S,9) CODE_S        		    		,");          //<CODE_S>附加编号</CODE_S>                    
                strExgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                strExgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>规格型号</G_MODEL>                  
                strExgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                strExgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                strExgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                strExgSql.Append("B.COUNTRY_CODE COUNTRY_CODE 	    ,");          //<COUNTRY_CODE>产销国</COUNTRY_CODE>         
                strExgSql.Append("B.DEC_PRICE DEC_PRICE         	,");          //<DEC_PRICE>企业申报单价</DEC_PRICE>            
                strExgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                strExgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                strExgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strExgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strExgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>            
                strExgSql.Append("B.FACTOR_RATE FACTOR_RATE         ,");          //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>      
                strExgSql.Append("B.QTY QTY         		        ,");          //<QTY>申报数量</QTY>                          
                strExgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                strExgSql.Append("B.FIRST_QTY FIRST_QTY         	,");          //<FIRST_QTY>初始库存数量</FIRST_QTY>            
                strExgSql.Append("B.USE_TYPE USE_TYPE         		,");          //<USE_TYPE>用途代码</USE_TYPE>                
                strExgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                strExgSql.Append("B.NOTE_2 NOTE_2         		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                strExgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>                          
                strExgSql.Append("B.MODIFY_MARK MODIFY_MARK         ,");          //<MODIFY_MARK>处理标志</MODIFY_MARK>          
                strExgSql.Append("'' APPR_AMT         		        ,");          //<APPR_AMT>总价</APPR_AMT>                  
                strExgSql.Append("'' G_ENG_NAME         		    ,");          //<G_ENG_NAME>英文名称</G_ENG_NAME>            
                strExgSql.Append("'' G_ENG_MODEL         		    ,");          //<G_ENG_MODEL>英文规格型号</G_ENG_MODEL>        
                strExgSql.Append("'' CLASS_NOTE         	        ,");          //<CLASS_NOTE>归类说明</CLASS_NOTE>            
                strExgSql.Append("'' COP_UNIT         		        ,");          //<COP_UNIT>企业自编计量单位</COP_UNIT>            
                strExgSql.Append("'' COP_FACTOR         	        ,");          //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>    
                strExgSql.Append("B.DUTY_MODE DUTY_MODE	            ,");           //<DUTY_MODE>征免方式</DUTY_MODE>              
                strExgSql.Append("'0' DUTY_RATE   ");           //<DUTY_RATE>非保税料件比例</DUTY_RATE>              
                strExgSql.Append("from                       	");
                strExgSql.Append("PRE_EMS3_CUS_HEAD A,PRE_EMS3_CUS_EXG B WHERE ");
                strExgSql.Append(" B.MODIFY_MARK!='0' ");
                strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" ORDER BY B.G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[1] != null)
                {
                    objRtnRult[1].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[1].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[1].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[1].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[1].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END
                /*
                 * 电子帐册料件
                 */
                StringBuilder strImgSql = new StringBuilder();

                #region 原电子账册料件查询语句 comment by ccx 2010-5-19
                //strImgSql.Append("select						");
                //strImgSql.Append("A.Ems_no         		,	");  								     /*	1.	帐册编号                  X(12)        */
                //strImgSql.Append("'0'        		,	");  										 /*	2.	变更次数               	  Z(8)9        */
                //strImgSql.Append("TO_CHAR(B.G_no,'FM999999990')           		,	");  			 /*	3.	帐册料件序号              Z(8)9        */
                //strImgSql.Append("B.Cop_g_no       		,	");  								     /*	4.	货号                      X(30)        */
                //strImgSql.Append("B.Code_t_s       		,	");  								     /*	5.	商品编码及附加商品编码    X(16)        */
                //strImgSql.Append("B.Class_mark     		,	");  								     /*	6.	归类标志                  X(1)         */
                //strImgSql.Append("B.G_name         		,	");  								     /*	7.	商品名称                  X(50)        */
                //strImgSql.Append("B.G_model        		,	");  								     /*	8.	商品规格型号              X(50)        */
                //strImgSql.Append("NVL(B.Unit,'000')           		,	");  						 /*	9.	申报计量单位              9(3)         */
                //strImgSql.Append("NVL(B.Unit_1,'000')         		,	");  						 /*	10.	法定计量单位              9(3)         */
                //strImgSql.Append("NVL(B.Unit_2,'000')         		,	");  						 /*	11.	法定第二单位              9(3)         */
                //strImgSql.Append("NVL(B.Country_code,'000')   		,	");  						 /*	12.	产销国(地区)              9(3)         */
                //strImgSql.Append("B.Source_mark    		,	");  								     /*	13.	来源标志                  X(1)         */
                //strImgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "')      		,	");  /*	14.	企业申报单价              Z(12)9.9(5)  */
                //strImgSql.Append("NVL(B.Curr,'000')           		,	");  						 /*	15.	币制                      9(3)         */
                //strImgSql.Append("TO_CHAR(NVL(B.Dec_price_rmb,0),'" + N13_5Format + "')  		,	");  /*	16.	申报单价人民币            Z(12)9.9(5)  */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_1,0),'" + N9_9Format + "')       		,	");  /*	17.	法定计量单位比例因子      Z(8)9.9(9)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_2,0),'" + N9_9Format + "')       		,	");  /*	18.	第二法定计量单位比例因子  Z(8)9.9(9)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_wt,0),'" + N9_9Format + "')      		,	");      /*	19.	重量比例因子              Z(8)9.9(9)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Factor_rate,0),'FM9990.00000')    		,	");  	 /*	20.	比例因子浮动比率          Z(3)9.9(5)   */
                //strImgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "')            		,	");  /*	21.	申报进口数量              Z(12)9.9(5)  */
                //strImgSql.Append("TO_CHAR(NVL(B.Max_qty,0),'" + N13_5Format + "')        		,	");  /*	22.	批准最大余量              Z(12)9.9(5)  */
                //strImgSql.Append("TO_CHAR(NVL(B.First_qty,0),'" + N13_5Format + "')      		,	");  /*	23.	初始库存数量              Z(12)9.9(5)  */
                //strImgSql.Append("B.I_e_type           	,	");  								     /*	24.	进/出口方式               X(1)         */
                //strImgSql.Append("B.Use_type           	,	");  								     /*	25.	用途代码                  Z(8)9        */
                //strImgSql.Append("B.Note_1             	,	");  								     /*	26.	备用标志1                 X(1)         */
                //strImgSql.Append("B.Note_2             	,	");  								     /*	27.	备用标志2                 X(1)         */
                //strImgSql.Append("B.Note               	,	");  								     /*	28.	备注                      X(10)        */
                //strImgSql.Append("B.Modify_mark        	    ");  								     /*	29	修改标志                  X(1)         */
                //strImgSql.Append("from                       	");
                //strImgSql.Append("PRE_EMS3_CUS_HEAD A,PRE_EMS3_CUS_IMG B WHERE ");
                //strImgSql.Append(" B.MODIFY_MARK!='0' ");
                //strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strImgSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                //strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strImgSql.Append(" ORDER BY B.G_NO");
                #endregion

                strImgSql.Append("select						");
                strImgSql.Append("B.G_NO G_NO        		    	,");		  //<G_NO>料件序号</G_NO>                        
                strImgSql.Append("B.COP_G_NO COP_G_NO        		,");          //<COP_G_NO>货号</COP_G_NO>                  
                strImgSql.Append("substr(B.CODE_T_S,0,9) CODE_T 	,");          //<CODE_T>商品编号</CODE_T>                    
                strImgSql.Append("substr(B.CODE_T_S,9) CODE_S   	,");          //<CODE_S>附加编号</CODE_S>                    
                strImgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                strImgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>规格型号</G_MODEL>                  
                strImgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                strImgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                strImgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                strImgSql.Append("B.COUNTRY_CODE COUNTRY_CODE 	    ,");          //<COUNTRY_CODE>产销国</COUNTRY_CODE>         
                strImgSql.Append("B.DEC_PRICE DEC_PRICE        	    ,");          //<DEC_PRICE>企业申报单价</DEC_PRICE>            
                strImgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                strImgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                strImgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                strImgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                strImgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>            
                strImgSql.Append("B.FACTOR_RATE FACTOR_RATE         ,");          //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>      
                strImgSql.Append("B.QTY QTY         		        ,");          //<QTY>申报数量</QTY>                          
                strImgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                strImgSql.Append("B.FIRST_QTY FIRST_QTY         	,");          //<FIRST_QTY>初始库存数量</FIRST_QTY>            
                strImgSql.Append("B.USE_TYPE USE_TYPE         		,");          //<USE_TYPE>用途代码</USE_TYPE>                
                strImgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                strImgSql.Append("B.NOTE_2 NOTE_2         		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                strImgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>                          
                strImgSql.Append("B.MODIFY_MARK MODIFY_MARK         ,");          //<MODIFY_MARK>处理标志</MODIFY_MARK>          
                strImgSql.Append("'' APPR_AMT         		        ,");          //<APPR_AMT>总价</APPR_AMT>                  
                strImgSql.Append("'' G_ENG_NAME         		    ,");          //<G_ENG_NAME>英文名称</G_ENG_NAME>            
                strImgSql.Append("'' G_ENG_MODEL         		    ,");          //<G_ENG_MODEL>英文规格型号</G_ENG_MODEL>        
                strImgSql.Append("'' CLASS_NOTE         	        ,");          //<CLASS_NOTE>归类说明</CLASS_NOTE>            
                strImgSql.Append("'' COP_UNIT         		        ,");          //<COP_UNIT>企业自编计量单位</COP_UNIT>            
                strImgSql.Append("'' COP_FACTOR         	        ,");          //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>    
                strImgSql.Append("B.DUTY_MODE DUTY_MODE	            ,");           //<DUTY_MODE>征免方式</DUTY_MODE>              
                strImgSql.Append("'0' DUTY_RATE   ");           //<DUTY_RATE>非保税料件比例</DUTY_RATE>              
                strImgSql.Append("from                       	");
                strImgSql.Append("PRE_EMS3_CUS_HEAD A,PRE_EMS3_CUS_IMG B WHERE ");
                strImgSql.Append(" B.MODIFY_MARK!='0' ");
                strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" ORDER BY B.G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                if (objRtnRult != null && objRtnRult[2] != null)
                {
                    objRtnRult[2].Columns.Add("VALUE_ADD_FIELD1");
                    objRtnRult[2].Columns.Add("VALUE_ADD_FIELD2");
                    objRtnRult[2].Columns.Add("CHAR_ADD_FIELD1");
                    objRtnRult[2].Columns.Add("CHAR_ADD_FIELD2");
                    objRtnRult[2].Columns.Add("DATE_ADD_FIELD");
                }
                //ADD END
                /*
                 * 电子帐册单损耗
                 */
                StringBuilder strConSql = new StringBuilder();

                #region 原电子账册单损耗查询语句 comment by ccx 2010-5-19
                //strConSql.Append("select				");
                //strConSql.Append("A.Ems_No         ,	");						/*1.		帐册编号    	X(12)      */
                //strConSql.Append("'0',	");										/*2.		变更次数    	Z(8)9      */
                //strConSql.Append("TO_CHAR(B.Exg_no,'FM099999999'),");			/*3.		成品序号    	9(9)       */
                //strConSql.Append("' '	        	,	");						/*4.		填充字段1   	X(21)      */
                //strConSql.Append("TO_CHAR(B.Exg_version,'FM999999990'),");		/*5.		成品版本    	Z(8)9      */
                //strConSql.Append("TO_CHAR(B.Img_no,'FM099999999'),");			/*6.		料件序号    	9(9)       */
                //strConSql.Append("' '	        	,	");						/*7.		填充字段2   	X(21)      */
                //strConSql.Append("TO_CHAR(NVL(B.Dec_cm,0),'" + N9_9Format + "'),"); /*8.		单耗        	Z(8)9.9(9) */
                //strConSql.Append("TO_CHAR(NVL(B.Dec_dm,0),'FM9990.00000'),");	/*9.		损耗        	Z(3)9.9(5) */
                //strConSql.Append("TO_CHAR(NVL(B.Other_cm,0),'" + N9_9Format + "'),");/*10.		其他单耗    	Z(8)9.9(9) */
                //strConSql.Append("TO_CHAR(NVL(B.Other_dm,0),'FM9990.00000'),");   /*11.		其他损耗    	Z(3)9.9(5) */
                //strConSql.Append("B.Cm_mark       	,	");						/*12.		单耗标志    	X(1)       */
                //strConSql.Append("B.Product_mark  	,	");						/*13.		加工流程标志	X(10)      */
                //strConSql.Append("B.Product_type  	,	");						/*14.		加工性质    	X(1)       */
                //strConSql.Append("B.Modify_mark   	,	");						/*15.		修改标志    	X(1)       */
                //strConSql.Append("' '				,	");						/*16.		布控标志    	X(1)       */
                //strConSql.Append("B.Note          	    ");						/*17.		备注        	X(10)      */
                //strConSql.Append("from                 ");
                //strConSql.Append("PRE_EMS3_CUS_HEAD A, PRE_EMS3_CUS_CONSUME B WHERE ");
                //strConSql.Append(" B.MODIFY_MARK!='0' ");
                //strConSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strConSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strConSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                //strConSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strConSql.Append(" ORDER BY B.EXG_NO,B.EXG_VERSION,B.IMG_NO");
                #endregion

                strConSql.Append("select				");
                strConSql.Append("B.EXG_NO              ,");		//<EXG_NO>成品序号</EXG_NO>           
                strConSql.Append("B.EXG_VERSION         ,");        //<EXG_VERSION>成品版本</EXG_VERSION> 
                strConSql.Append("B.IMG_NO              ,");        //<IMG_NO>料件序号</IMG_NO>           
                strConSql.Append("B.DEC_CM              ,");        //<DEC_CM>净耗</DEC_CM>             
                strConSql.Append("B.DEC_DM              ,");        //<DEC_DM>损耗率%</DEC_DM>           
                strConSql.Append("B.MODIFY_MARK         ,");        //<MODIFY_MARK>处理标志</MODIFY_MARK>
                strConSql.Append("B.NOTE                ,");         //<NOTE>备注</NOTE>           
                //ADD BY LSK
                strConSql.Append("B.NON_MNL_RATIO       ");         //<NON_MNL_RATIO>非保税料件比例%</NON_MNL_RATIO>
                //ADD END
                strConSql.Append("from                  ");
                strConSql.Append("PRE_EMS3_CUS_HEAD A, PRE_EMS3_CUS_CONSUME B WHERE ");
                strConSql.Append(" B.MODIFY_MARK!='0' ");
                //strConSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strConSql.Append(" AND B.COP_EMS_NO = A.COP_EMS_NO");
                //strConSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strConSql.Append(" AND B.TRADE_CODE = A.TRADE_CODE");
                strConSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                strConSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strConSql.Append(" ORDER BY B.EXG_NO,B.EXG_VERSION,B.IMG_NO");

                bErrCode = Ems3Data.readTableByAdapter(strConSql.ToString(), QryExp, asaConn, out objRtnRult[3]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                //ADD BY LSK
                //objRtnRult[3].Columns.Add("NON_MNL_RATIO");
                if (objRtnRult != null && objRtnRult[3] != null)
                {
                    objRtnRult[3].Columns.Add("VALUE_ADD_FIELD");
                    objRtnRult[3].Columns.Add("CHAR_ADD_FIELD");
                }
                //ADD END
                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                  DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }
        public static int getFasDataOfMsg(//电子帐册分册
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {//错误信息
            /*
             * 分册表头
             */
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);

            strHeadSql.Append("select				");
            strHeadSql.Append("Ems_no        ,		");                                   	  /*1.		帐册编号    	X(12)      */
            strHeadSql.Append("Fascicle_no   ,		");                                   	  /*2.		分册号      	X(12)      */
            strHeadSql.Append("'0'       ,		");                               	  /*3.		变更次数    	Z(8)9      */
            strHeadSql.Append("i_e_port      ,		");                                   	  /*4.		进出口岸    	9(4)       */
            strHeadSql.Append("TO_CHAR(limit_date,'YYYYMMDD'),");                              /*5.		该分册的期限	Z(8)       */
            strHeadSql.Append("Cop_ems_no    ,		");                                   	  /*6.		企业内部编号	X(20)      */
            strHeadSql.Append("Trade_code    ,		");                                   	  /*7.		经营单位代码	X(10)      */
            strHeadSql.Append("declare_code  ,		");                                   	  /*8.		申报单位代码	X(10)      */
            strHeadSql.Append("id_card       ,		");                                   	  /*9.		身份识别号  	X(20)      */
            strHeadSql.Append("Id_card_pwd   ,		");                                   	  /*10.		身份识别密码	X(20)      */
            strHeadSql.Append("Process_mark  ,		");                                   	  /*11.		处理标志    	X(10)      */
            strHeadSql.Append("TO_CHAR(Input_date,'YYYYMMDD')    ,		");               	  /*12.		录入日期    	Z(8)       */
            strHeadSql.Append("TO_CHAR(NVL(Input_er,0),'FM0000')      ,		");           	  /*13.		录入员代号  	Z(4)       */
            strHeadSql.Append("TO_CHAR(Declare_date,'YYYYMMDD')  ,		");               	  /*14.		申报日期    	Z(8)       */
            strHeadSql.Append("TO_CHAR(Declare_date,'HH24MMss')    ,		");               /*15.		申报时间    	Z(8)       */
            strHeadSql.Append("Declare_type  ,		");                                       /*16.		申报类型    	X(1)       */
            strHeadSql.Append("Fascicle_type ,		");                                       /*17.		分册类型    	X(1)       */
            strHeadSql.Append("Declare_mark  ,		");                                       /*18.		申报标志    	X(1)       */
            strHeadSql.Append("Chk_mark      ,		");                                       /*19.		审批标志    	X(1)       */
            strHeadSql.Append("Exe_mark      ,		");                                       /*20.		执行标志    	X(1)       */
            strHeadSql.Append("TO_CHAR(New_appr_date,'YYYYMMDD') ,		");                   /*21.		备案批准日期	Z(8)       */
            strHeadSql.Append("TO_CHAR(Cng_appr_date,'YYYYMMDD') ,		");                   /*22.		变更批准日期	Z(8)       */
            strHeadSql.Append("TO_CHAR(Print_date,'YYYYMMDD')    ,		");                   /*23.		打印日期    	Z(8)       */
            strHeadSql.Append("TO_CHAR(Print_date,'HH24MMSS')    ,		");                   /*24.		打印时间    	Z(8)       */
            strHeadSql.Append("Print_mark    ,		");                                       /*25.		打印标志    	X(1)       */
            strHeadSql.Append("TO_CHAR(NVL(img_items,0),'FM099999999')     ,		");  	  /*26.		进口原料项数	9(9)       */
            strHeadSql.Append("TO_CHAR(NVL(exg_items,0),'FM099999999')     ,		");       /*27.		出口原料项数	9(9)       */
            strHeadSql.Append("note_1        ,		");                                       /*28.		备用标志1   	X(1)       */
            strHeadSql.Append("note_2        ,		");                                       /*29.		备用标志2   	X(1)       */
            strHeadSql.Append("TO_CHAR(NVL(Note_amount,0),'" + N13_5Format + "')   ,		");   /*30.		备用金额    	Z(12)9.9(5)*/
            strHeadSql.Append("TO_CHAR(NVL(Note_qty,0),'" + N13_5Format + "')      ,		");   /*31.		备用数量    	Z(12)9.9(5)*/
            strHeadSql.Append("Note          ,		");                                       /*32.		备注        	X(10)      */
            strHeadSql.Append("Modify_mark    		");                                       /*33.		修改标志    	X(1)       */
            strHeadSql.Append("FROM PRE_EMS3_FAS_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
         //   OracleConnection asaConn = DataManager.GetBizConn();
            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[3];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 分册料件
                 */
                StringBuilder strImgSql = new StringBuilder();
                strImgSql.Append("select			  ");
                strImgSql.Append("A.Ems_no       	, ");							 /*1.		帐册编号    	X(12)      */
                strImgSql.Append("A.Fascicle_no  	, ");							 /*2.		分册号      	X(12)      */
                strImgSql.Append("'0' 	, ");										 /*3.		变更次数    	Z(8)9      */
                strImgSql.Append("TO_CHAR(B.G_no,'FM999999990'),");                  /*4.		帐册料件序号	Z(8)9      */
                strImgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "'),");        /*5.		允许数量    	Z(12)9.9(5)*/
                strImgSql.Append("TO_CHAR(NVL(B.Note_qty,0),'" + N13_5Format + "'),");   /*6.		备用数量    	Z(12)9.9(5)*/
                strImgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "'),");  /*7.		企业申报单价	Z(12)9.9(5)*/
                strImgSql.Append("NVL(B.Curr,'000'),");								 /*8.		币制        	9(3)       */
                strImgSql.Append("B.Note_1       	, ");							 /*9.		备用标志1   	X(1)       */
                strImgSql.Append("B.Note_2       	, ");							 /*10.	    备用标志2   	X(1)       */
                strImgSql.Append("B.Modify_mark  	, ");							 /*11.	    修改标志    	X(1)       */
                strImgSql.Append("B.Note         	  ");							 /*12.	    备注        	X(10)      */
                strImgSql.Append("from                       	");
                strImgSql.Append("PRE_EMS3_FAS_HEAD A, PRE_EMS3_FAS_IMG B WHERE ");
                strImgSql.Append(" B.MODIFY_MARK!='0' ");
                strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strImgSql.Append(" ORDER BY B.G_NO");
                bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 分册成品
                 */
                StringBuilder strExgSql = new StringBuilder();
                strExgSql.Append("select			  ");
                strExgSql.Append("A.Ems_no       	, ");							 /*1.		帐册编号    	X(12)      */
                strExgSql.Append("A.Fascicle_no  	, ");							 /*2.		分册号      	X(12)      */
                strExgSql.Append("'0' 	, ");										 /*3.		变更次数    	Z(8)9      */
                strExgSql.Append("TO_CHAR(B.G_no,'FM999999990'),");                  /*4.		帐册料件序号	Z(8)9      */
                strExgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N13_5Format + "'),");        /*5.		允许数量    	Z(12)9.9(5)*/
                strExgSql.Append("TO_CHAR(NVL(B.Note_qty,0),'" + N13_5Format + "'),");   /*6.		备用数量    	Z(12)9.9(5)*/
                strExgSql.Append("TO_CHAR(NVL(B.Dec_price,0),'" + N13_5Format + "'),");  /*7.		企业申报单价	Z(12)9.9(5)*/
                strExgSql.Append("NVL(B.Curr,'000'),");								 /*8.		币制        	9(3)       */
                strExgSql.Append("B.Note_1       	, ");							 /*9.		备用标志1   	X(1)       */
                strExgSql.Append("B.Note_2       	, ");							 /*10.	    备用标志2   	X(1)       */
                strExgSql.Append("B.Modify_mark  	, ");							 /*11.	    修改标志    	X(1)       */
                strExgSql.Append("B.Note         	  ");							 /*12.	    备注        	X(10)      */
                strExgSql.Append("from                       	");
                strExgSql.Append("PRE_EMS3_FAS_HEAD A, PRE_EMS3_FAS_EXG B WHERE ");
                strExgSql.Append(" B.MODIFY_MARK!='0' ");
                strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" AND A.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" ORDER BY B.G_NO");
                bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }
                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                  DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }
        public static int getDcrDataOfMsg(//预(正式)报核
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {//错误信息
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);
            QryExp.Add("DCR_TIMES", strOtherPara);

            #region 原账册报核表头查询语句 comment by ccx 2010-5-19
            //strHeadSql.Append("select		   ");
            //strHeadSql.Append("Ems_no        , ");									/*1.		帐册编号        	X(12)      */
            //strHeadSql.Append("TO_CHAR(NVL(dcr_times,0),'FM999999990'),");			/*2.		报核次数        	Z(8)9      */
            //strHeadSql.Append("Dcr_type      , ");									/*3.		报核类型        	X(1)       */
            //strHeadSql.Append("TO_CHAR(Begin_date,'YYYYMMDD'),");					/*4.		报核开始日期    	Z(8)       */
            //strHeadSql.Append("TO_CHAR(End_date,'YYYYMMDD'),");						/*5.		报核截至日期    	Z(8)       */
            //strHeadSql.Append("TO_CHAR(NVL(Entry_I_num,0),'FM999999990'),");		/*6.		进口报关单总份数	Z(8)9      */
            //strHeadSql.Append("TO_CHAR(NVL(Entry_e_num,0),'FM999999990'), ");		/*7.		出口报关单总份数	Z(8)9      */
            //strHeadSql.Append("TO_CHAR(NVL(Img_num,0),'FM999999990'), ");			/*8.		报核料件总项数  	Z(8)9      */
            //strHeadSql.Append("TO_CHAR(NVL(Exg_num,0),'FM999999990'), ");			/*9.		报核成品总项数  	Z(8)9      */
            //strHeadSql.Append("TO_CHAR(NVL(Imr_num,0),'FM999999990'), ");			/*10.		报核边角料总项数	Z(8)9      */
            //strHeadSql.Append("TO_CHAR(NVL(Exr_num,0),'FM999999990'), ");			/*11.		报核残次品总项数	Z(8)9      */
            //strHeadSql.Append("Iccard_id     , ");									/*12.		身份识别号      	X(20)      */
            //strHeadSql.Append("Id_card_pwd   , ");									/*13.		身份识别密码    	X(20)      */
            //strHeadSql.Append("TO_CHAR(Input_date,'YYYYMMDD'),");					/*14.		录入日期        	Z(8)       */
            //strHeadSql.Append("TO_CHAR(NVL(Input_er,0),'FM0000'),");				/*15.		录入员代号      	9(4)       */
            //strHeadSql.Append("TO_CHAR(Dcr_date,'YYYYMMDD'),");						/*16.		报核申报日期    	Z(8)       */
            //strHeadSql.Append("TO_CHAR(Dcr_date,'HH24MMss'),");						/*17.		报核申报时间    	Z(8)       */
            //strHeadSql.Append("Ems_appr_mark , ");									/*18.		其它部门审批标志	X(10)      */
            //strHeadSql.Append("'          '  , ");									/*19.		其它单证标志    	X(10)      */
            //strHeadSql.Append("Note_1        , ");									/*20.		备用标志1       	X(1)       */
            //strHeadSql.Append("TO_CHAR(NVL(Note_amount,0),'" + N13_5Format + "'),");	/* 21	:	备用金额			Z(12)9.9(5)*/
            //strHeadSql.Append("TO_CHAR(NVL(Note_qty,0),'" + N13_5Format + "'),");		/*22.		备用数量        	Z(12)9.9(5)*/
            //strHeadSql.Append("note            ");									/*23.		备注            	X(30)      */
            //strHeadSql.Append("FROM PRE_EMS3_DCR_HEAD WHERE ");
            //strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            //strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
            //strHeadSql.Append(" AND DCR_TIMES = {DCR_TIMES}");
            #endregion

            strHeadSql.Append("select		                                    ");
            strHeadSql.Append("EMS_NO                                  			,");	  //<EMS_NO>帐册编号</EMS_NO>              		
            strHeadSql.Append("DCR_TIMES                                        ,");      //<DCR_TIMES>报核次数</DCR_TIMES>        
            strHeadSql.Append("DCR_TYPE                                         ,");      //<DCR_TYPE>报核类型</DCR_TYPE>          
            strHeadSql.Append("CONVERT(VARCHAR,BEGIN_DATE,112) BEGIN_DATE       ,");      //<BEGIN_DATE>报核开始日期</BEGIN_DATE>    
            strHeadSql.Append("CONVERT(VARCHAR,END_DATE,112) END_DATE           ,");      //<END_DATE>报核截至日期</END_DATE>        
            strHeadSql.Append("'0' DECL_IN_AMT                                   ,");      //<DECL_IN_AMT>料件进口总金额</DECL_IN_AMT> 
            strHeadSql.Append("'0' DECL_EX_AMT                                   ,");      //<DECL_EX_AMT>成品出口总金额</DECL_EX_AMT> 
            strHeadSql.Append("ENTRY_I_NUM I_DEC_QTY                            ,");      //<I_DEC_QTY>进口报关单总份数</I_DEC_QTY>    
            strHeadSql.Append("ENTRY_E_NUM E_DEC_QTY                            ,");      //<E_DEC_QTY>出口报关单总份数</E_DEC_QTY>    
            strHeadSql.Append("IMG_NUM                                          ,");      //<IMG_NUM>报核料件总项数</IMG_NUM>         
            strHeadSql.Append("EXG_NUM                                          ,");      //<EXG_NUM>报核成品总项数</EXG_NUM>         
            strHeadSql.Append("IMR_NUM                                          ,");      //<IMR_NUM>报核边角料总项数</IMR_NUM>        
            strHeadSql.Append("EXR_NUM                                          ,");      //<EXR_NUM>报核残次品总项数</EXR_NUM>        
            strHeadSql.Append("CONVERT(VARCHAR,INPUT_DATE,112) INPUT_DATE       ,");      //<INPUT_DATE>录入日期</INPUT_DATE>      
            strHeadSql.Append("INPUT_ER                                         ,");      //<INPUT_ER>录入员代号</INPUT_ER>         
            strHeadSql.Append("CONVERT(VARCHAR,DCR_DATE,112) DCR_DATE           ,");      //<DCR_DATE>报核申报日期</DCR_DATE>        
            strHeadSql.Append("'' DCR_TIME                                      ,");      //<DCR_TIME>报核申报时间</DCR_TIME>        
            strHeadSql.Append("NOTE_1                                           ,");      //<NOTE_1>备用标志1</NOTE_1>             
            strHeadSql.Append("NOTE_AMOUNT                                      ,");      //<NOTE_AMOUNT>备用金额</NOTE_AMOUNT>    
            strHeadSql.Append("NOTE_QTY                                         ,");      //<NOTE_QTY>备用数量</NOTE_QTY>          
            strHeadSql.Append("NOTE                                           	");       //<NOTE>备注</NOTE>                    
            strHeadSql.Append("FROM PRE_EMS3_DCR_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
            strHeadSql.Append(" AND DCR_TIMES = {DCR_TIMES}");

            //  OracleConnection asaConn = DataManager.GetBizConn();
            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[6];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 报核报关单
                 */
                if (strMsgType == "EMS231")		//正式报核不发送报关单
                {
                    objRtnRult[1] = null;
                }
                else							//非正式报核发送报关单
                {
                    StringBuilder strEntSql = new StringBuilder();

                    #region 原账册报核报关单查询语句 comment by ccx 2010-5-19
                    //strEntSql.Append("select						 ");
                    //strEntSql.Append("A.Ems_No         			,");							/*1.		帐册编号      	X(12)	*/
                    //strEntSql.Append("TO_CHAR(NVL(B.dcr_times,0),'FM999999990'),");			    /*2.		报核次数      	Z(8)9	*/
                    //strEntSql.Append("SUBSTRB(B.Entry_id,-9,9),");								/*3.		报关单号      	Z(8)9	*/
                    //strEntSql.Append("NVL(B.Master_customs,'0000'),");							/*4.		报关地海关代码	9(4) 	*/
                    //strEntSql.Append("B.I_e_flag        			,");						/*5.		进出口标志    	X(1) 	*/
                    //strEntSql.Append("TO_CHAR(NVL(B.Items_num,0),'FM999999990'),");				/*6.		商品项数      	Z(8)9	*/
                    //strEntSql.Append("TO_CHAR(B.Declare_date,'YYYYMMDD'),");					/*7.		申报日期      	Z(8) 	*/
                    //strEntSql.Append("TO_CHAR(B.I_e_date,'YYYYMMDD'),");						/*8.		进出日期      	Z(8) 	*/
                    //strEntSql.Append("B.Du_code         			,");						/*9.		核扣/通关方式 	X(2) 	*/
                    //strEntSql.Append("B.Ent_mark        			,");						/*10.		审核标志  	  	X(1) 	*/
                    //strEntSql.Append("B.Note            			,");						/*11.		备注      	  	X(10)	*/
                    //strEntSql.Append("B.Modify_mark       		     ");						/*12.		修改标志  	  	X(1) 	*/
                    //strEntSql.Append("from                          ");
                    //strEntSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_DCR_ENT B WHERE ");
                    //strEntSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                    //strEntSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                    //strEntSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                    //strEntSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                    //strEntSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                    //strEntSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                    //strEntSql.Append(" ORDER BY B.ENTRY_ID");
                    #endregion

                    strEntSql.Append("select						 ");
                    strEntSql.Append("B.ENTRY_ID I_E_DEC_NO         					,");	  //<I_E_DEC_NO>报关单号</I_E_DEC_NO>           
                    strEntSql.Append("B.MASTER_CUSTOMS CUSTOM_MASTER         			,");      //<CUSTOM_MASTER>报关地海关代码</CUSTOM_MASTER> 
                    strEntSql.Append("B.I_E_FLAG I_E_FLAG         						,");      //<I_E_FLAG>进出口标志</I_E_FLAG>             
                    strEntSql.Append("CONVERT(VARCHAR,B.DECLARE_DATE,112) DECLARE_DATE  ,");      //<DECLARE_DATE>申报日期</DECLARE_DATE>      
                    strEntSql.Append("CONVERT(VARCHAR,B.I_E_DATE,112) I_E_DATE         	,");      //<I_E_DATE>进出日期</I_E_DATE>              
                    strEntSql.Append("isnull(B.DU_CODE,'0') DU_CODE         			,");      //<DU_CODE>核扣/通关方式</DU_CODE>             
                    strEntSql.Append("B.ENT_MARK ENT_MARK         						,");      //<ENT_MARK>审核标志</ENT_MARK>             
                    strEntSql.Append("B.NOTE NOTE         								");       //<NOTE>备注</NOTE>                        
                    strEntSql.Append("from                          ");
                    strEntSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_DCR_ENT B WHERE ");
                    strEntSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                    strEntSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                    strEntSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                    strEntSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                    strEntSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                    strEntSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                    strEntSql.Append(" ORDER BY B.ENTRY_ID");

                    bErrCode = Ems3Data.readTableByAdapter(strEntSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                    if (bErrCode != 0)
                    {
                        strErrMsg = "查询失败！";
                        return bErrCode;
                    }
                }

                /*
                 * 报核料件
                 */
                StringBuilder strDcrImgSql = new StringBuilder();

                #region 原账册报核料件查询语句 comment by ccx 2010-5-19
                //strDcrImgSql.Append("select						");
                //strDcrImgSql.Append("A.Ems_no                ,	");								 /*1.		帐册编号             X(12)       	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.dcr_times,0),'FM999999990'),");				 /*2.		报核次数           	 Z(8)9       	*/
                //strDcrImgSql.Append("TO_CHAR(B.G_no,'FM000000000'),");							 /*3.		料件/成品项号      	 9(9)        	*/
                //strDcrImgSql.Append("' '	                   ,	");							 /*4.		填充字段           	 X(21)       	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Gr_qty,0),'" + N12_5Format + "'),");				 /*5.		边角料/残次品数量  	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Gr_amount,0),'" + N12_5Format + "'),");			 /*6.		边角料/残次品总价值	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Gr_weight,0),'" + N12_5Format + "'),");			 /*7.		边角料/残次品总重量	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Remain_qty,0),'" + N12_5Format + "'),");			 /*8.		应剩余数量         	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Remain_amount,0),'" + N12_5Format + "'),");		 /*9.		应剩余总价值       	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Remain_weight,0),'" + N12_5Format + "'),");		 /*10.		应剩余总重量       	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Cm_qty,0),'" + N12_5Format + "'),");				 /*11.		消耗总数量         	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Cm_amount,0),'" + N12_5Format + "'),");			 /*12.		消耗总价值         	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Cm_weight,0),'" + N12_5Format + "'),");			 /*13.		消耗总重量         	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Fact_Remain_qty,0),'" + N12_5Format + "'),");	 /*14.		实际剩余数量       	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Fact_Remain_amount,0),'" + N12_5Format + "'),");  /*15.		实际剩余总价值     	 +9(12).9(5)	*/
                //strDcrImgSql.Append("TO_CHAR(NVL(B.Fact_Remain_weight,0),'" + N12_5Format + "'),");  /*16.		实际剩余总重量     	 +9(12).9(5)	*/
                //strDcrImgSql.Append("B.Note_1                ,	");								 /*17.		备用标志1          	 X(1)        	*/
                //strDcrImgSql.Append("B.Note_2                ,	");								 /*18.		备用标志2          	 X(1)        	*/
                //strDcrImgSql.Append("B.Note                  ,	");								 /*19.		备注               	 X(10)       	*/
                //strDcrImgSql.Append("B.Modify_mark            	");								 /*20.		修改标志           	 X(1)        	*/
                //strDcrImgSql.Append("from                        ");
                //strDcrImgSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_DCR_IMG B WHERE ");
                //strDcrImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strDcrImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strDcrImgSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                //strDcrImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strDcrImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strDcrImgSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                //strDcrImgSql.Append(" ORDER BY B.G_NO");
                #endregion
  
                strDcrImgSql.Append("select						    ");
                strDcrImgSql.Append("B.G_NO G_NO                    ,");        //<G_NO>料件序号</G_NO> 		
                strDcrImgSql.Append("C.COP_G_NO COP_G_NO            ,");	    //<COP_G_NO>料件料号</COP_G_NO> 	
                //strDcrImgSql.Append("'0' QTY_TYPE                   ,");	    //<QTY_TYPE>报核数量类型</QTY_TYPE>	
                strDcrImgSql.Append("B.GR_QTY GR_QTY                ,");	    //<QTY>01	边角料/残次品数量</QTY>
                strDcrImgSql.Append("B.GR_AMOUNT GR_AMOUNT          ,");	    //<QTY>02	边角料/残次品总价值</QTY>
                strDcrImgSql.Append("B.GR_WEIGHT GR_WEIGHT          ,");	    //<QTY>03	边角料/残次品总重量</QTY>
                strDcrImgSql.Append("B.REMAIN_QTY REMAIN_QTY        ,");	    //<QTY>04	应剩余数量</QTY>
                strDcrImgSql.Append("B.REMAIN_AMOUNT REMAIN_AMOUNT  ,");	    //<QTY>05	应剩余总价值</QTY>
                strDcrImgSql.Append("B.REMAIN_WEIGHT REMAIN_WEIGHT  ,");	    //<QTY>06	应剩余总重量</QTY>
                strDcrImgSql.Append("B.CM_QTY CM_QTY                ,");	    //<QTY>07	消耗总数量</QTY>
                strDcrImgSql.Append("B.CM_AMOUNT CM_AMOUNT          ,");	    //<QTY>08	消耗总价值</QTY>
                strDcrImgSql.Append("B.CM_WEIGHT CM_WEIGHT          ,");	    //<QTY>09	消耗总重量</QTY>
                strDcrImgSql.Append("B.FACT_REMAIN_QTY FACT_REMAIN_QTY                ,");	    //<QTY>10	实际剩余数量</QTY>
                strDcrImgSql.Append("B.FACT_REMAIN_AMOUNT FACT_REMAIN_AMOUNT          ,");	    //<QTY>11	实际剩余总价值</QTY>
                strDcrImgSql.Append("B.FACT_REMAIN_WEIGHT FACT_REMAIN_WEIGHT          ");	    //<QTY>12	实际剩余总重量</QTY>
                strDcrImgSql.Append("from                           ");
                strDcrImgSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_DCR_IMG B, CUR_EMS3_CUS_IMG C WHERE ");
                strDcrImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strDcrImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strDcrImgSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                strDcrImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strDcrImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strDcrImgSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                strDcrImgSql.Append(" AND C.G_NO = B.G_NO");
                strDcrImgSql.Append(" AND C.COP_EMS_NO = {COP_EMS_NO}");
                strDcrImgSql.Append(" AND C.TRADE_CODE = {TRADE_CODE}");
                strDcrImgSql.Append(" ORDER BY B.G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strDcrImgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 报核成品
                 */
                StringBuilder strDcrExgSql = new StringBuilder();

                #region 原账册报核成品查询语句 comment by ccx 2010-5-19
                //strDcrExgSql.Append("select						");
                //strDcrExgSql.Append("A.Ems_no                ,	");								 /*1.		帐册编号           	 X(12)       	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.dcr_times,0),'FM999999990'),");				 /*2.		报核次数           	 Z(8)9       	*/
                //strDcrExgSql.Append("TO_CHAR(B.G_no,'FM000000000'),");							 /*3.		料件/成品项号      	 9(9)        	*/
                //strDcrExgSql.Append("' '	                   ,	");							 /*4.		填充字段           	 X(21)       	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Gr_qty,0),'" + N12_5Format + "'),");				 /*5.		边角料/残次品数量  	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Gr_amount,0),'" + N12_5Format + "'),");			 /*6.		边角料/残次品总价值	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Gr_weight,0),'" + N12_5Format + "'),");			 /*7.		边角料/残次品总重量	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Remain_qty,0),'" + N12_5Format + "'),");			 /*8.		应剩余数量         	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Remain_amount,0),'" + N12_5Format + "'),");		 /*9.		应剩余总价值       	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Remain_weight,0),'" + N12_5Format + "'),");		 /*10.		应剩余总重量       	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Cm_qty,0),'" + N12_5Format + "'),");				 /*11.		消耗总数量         	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Cm_amount,0),'" + N12_5Format + "'),");			 /*12.		消耗总价值         	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Cm_weight,0),'" + N12_5Format + "'),");			 /*13.		消耗总重量         	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Fact_Remain_qty,0),'" + N12_5Format + "'),");	 /*14.		实际剩余数量       	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Fact_Remain_amount,0),'" + N12_5Format + "'),");  /*15.		实际剩余总价值     	 +9(12).9(5)	*/
                //strDcrExgSql.Append("TO_CHAR(NVL(B.Fact_Remain_weight,0),'" + N12_5Format + "'),");  /*16.		实际剩余总重量     	 +9(12).9(5)	*/
                //strDcrExgSql.Append("B.Note_1                ,	");								 /*17.		备用标志1          	 X(1)        	*/
                //strDcrExgSql.Append("B.Note_2                ,	");								 /*18.		备用标志2          	 X(1)        	*/
                //strDcrExgSql.Append("B.Note                  ,	");								 /*19.		备注               	 X(10)       	*/
                //strDcrExgSql.Append("B.Modify_mark            	");								 /*20.		修改标志           	 X(1)        	*/
                //strDcrExgSql.Append("from                        ");
                //strDcrExgSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_DCR_EXG B WHERE ");
                //strDcrExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strDcrExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strDcrExgSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                //strDcrExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strDcrExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strDcrExgSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                //strDcrExgSql.Append(" ORDER BY B.G_NO");
                #endregion

                strDcrExgSql.Append("select						    ");
                strDcrExgSql.Append("B.G_NO G_NO                    ,");    //<G_NO>成品序号</G_NO> 		
                strDcrExgSql.Append("C.COP_G_NO COP_G_NO            ,");	//<COP_G_NO>成品料号</COP_G_NO> 	
                //strDcrExgSql.Append("'0' QTY_TYPE                    ,");	//<QTY_TYPE>报核数量类型</QTY_TYPE>	
                strDcrExgSql.Append("B.GR_QTY GR_QTY                ,");	    //<QTY>01	边角料/残次品数量</QTY>
                strDcrExgSql.Append("B.GR_AMOUNT GR_AMOUNT          ,");	    //<QTY>02	边角料/残次品总价值</QTY>
                strDcrExgSql.Append("B.GR_WEIGHT GR_WEIGHT          ,");	    //<QTY>03	边角料/残次品总重量</QTY>
                strDcrExgSql.Append("B.REMAIN_QTY REMAIN_QTY        ,");	    //<QTY>04	应剩余数量</QTY>
                strDcrExgSql.Append("B.REMAIN_AMOUNT REMAIN_AMOUNT  ,");	    //<QTY>05	应剩余总价值</QTY>
                strDcrExgSql.Append("B.REMAIN_WEIGHT REMAIN_WEIGHT  ,");	    //<QTY>06	应剩余总重量</QTY>
                strDcrExgSql.Append("B.CM_QTY CM_QTY                ,");	    //<QTY>07	消耗总数量</QTY>
                strDcrExgSql.Append("B.CM_AMOUNT CM_AMOUNT          ,");	    //<QTY>08	消耗总价值</QTY>
                strDcrExgSql.Append("B.CM_WEIGHT CM_WEIGHT          ,");	    //<QTY>09	消耗总重量</QTY>
                strDcrExgSql.Append("B.FACT_REMAIN_QTY FACT_REMAIN_QTY                ,");	    //<QTY>10	实际剩余数量</QTY>
                strDcrExgSql.Append("B.FACT_REMAIN_AMOUNT FACT_REMAIN_AMOUNT          ,");	    //<QTY>11	实际剩余总价值</QTY>
                strDcrExgSql.Append("B.FACT_REMAIN_WEIGHT FACT_REMAIN_WEIGHT           ");	    //<QTY>12	实际剩余总重量</QTY>
                
                //strDcrExgSql.Append("B.NOTE_1 I_E_QTY          ");
                strDcrExgSql.Append("from                           ");
                strDcrExgSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_DCR_EXG B, CUR_EMS3_CUS_EXG C WHERE ");
                strDcrExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strDcrExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strDcrExgSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                strDcrExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strDcrExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strDcrExgSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                strDcrExgSql.Append(" AND C.G_NO = B.G_NO");
                strDcrExgSql.Append(" AND C.COP_EMS_NO = {COP_EMS_NO}");
                strDcrExgSql.Append(" AND C.TRADE_CODE = {TRADE_CODE}");
                strDcrExgSql.Append(" ORDER BY B.G_NO");
                
                bErrCode = Ems3Data.readTableByAdapter(strDcrExgSql.ToString(), QryExp, asaConn, out objRtnRult[3]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                #region 原账册报核核算料件、成品查询语句 comment by ccx 2010-5-19
                ///*
                // * 核算料件
                // */
                //StringBuilder strClsImgSql = new StringBuilder();
                //strClsImgSql.Append("select									   ");
                //strClsImgSql.Append("A.Ems_No     							,  ");		/*1.		帐册编号      	X(12)      	*/
                //strClsImgSql.Append("TO_CHAR(NVL(B.dcr_times,0),'FM999999990'),");		/*2.		报核次数      	Z(8)9      	*/
                //strClsImgSql.Append("TO_CHAR(B.G_no,'FM000000000'),");					/*3.		项号          	9(9)       	*/
                //strClsImgSql.Append("' '          							,  ");		/*4.		填充字段      	X(21)      	*/
                //strClsImgSql.Append("B.Du_code    							,  ");		/*5.		核扣方式      	X(2)       	*/
                //strClsImgSql.Append("TO_CHAR(NVL(B.Items_no,0),'FM999999990'),");		/*6.		报关单商品项数	Z(8)9      	*/
                //strClsImgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N12_5Format + "'),");		/*7.		数量          	+9(12).9(5)	*/
                //strClsImgSql.Append("TO_CHAR(NVL(B.Amount,0),'" + N12_5Format + "'),");		/*8.		金额          	+9(12).9(5)	*/
                //strClsImgSql.Append("TO_CHAR(NVL(B.Weight,0),'" + N13_5Format + "'),");		/*9.		重量          	Z(12)9.9(5)	*/
                //strClsImgSql.Append("DECODE(B.Du_mark,'+','+','-','-',' ')    ,  ");	/*10.		核扣方法      	X(1)       	*/
                //strClsImgSql.Append("B.Note_1     							,  ");		/*11.		备用标志1     	X(1)       	*/
                //strClsImgSql.Append("B.Note          							   ");  /*12.		备注          	X(10)      	*/
                //strClsImgSql.Append("from            							   ");
                //strClsImgSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_CLR_IMG B  WHERE   ");
                //strClsImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strClsImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strClsImgSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                //strClsImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strClsImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strClsImgSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                //strClsImgSql.Append(" ORDER BY B.G_NO,B.DU_CODE");

                //bErrCode = Ems3Data.readTableByAdapter(strClsImgSql.ToString(), QryExp, asaConn, out objRtnRult[4]);
                //if (bErrCode != 0)
                //{
                //    strErrMsg = "查询失败！";
                //    return bErrCode;
                //}

                ///*
                // * 核算成品
                // */
                //StringBuilder strClsExgSql = new StringBuilder();
                //strClsExgSql.Append("select									   ");
                //strClsExgSql.Append("A.Ems_No     							,  ");		/*1.		帐册编号      	X(12)      	*/
                //strClsExgSql.Append("TO_CHAR(NVL(B.dcr_times,0),'FM999999990'),");		/*2.		报核次数      	Z(8)9      	*/
                //strClsExgSql.Append("TO_CHAR(B.G_no,'FM000000000'),");					/*3.		项号          	9(9)       	*/
                //strClsExgSql.Append("' '          							,  ");		/*4.		填充字段      	X(21)      	*/
                //strClsExgSql.Append("B.Du_code    							,  ");		/*5.		核扣方式      	X(2)       	*/
                //strClsExgSql.Append("TO_CHAR(NVL(B.Items_no,0),'FM999999990'),");		/*6.		报关单商品项数	Z(8)9      	*/
                //strClsExgSql.Append("TO_CHAR(NVL(B.Qty,0),'" + N12_5Format + "'),");		/*7.		数量          	+9(12).9(5)	*/
                //strClsExgSql.Append("TO_CHAR(NVL(B.Amount,0),'" + N12_5Format + "'),");		/*8.		金额          	+9(12).9(5)	*/
                //strClsExgSql.Append("TO_CHAR(NVL(B.Weight,0),'" + N13_5Format + "'),");		/*9.		重量          	Z(12)9.9(5)	*/
                //strClsExgSql.Append("DECODE(B.Du_mark,'+','+','-','-',' ')    ,  ");	/*10.		核扣方法      	X(1)       	*/
                //strClsExgSql.Append("B.Note_1     							,  ");		/*11.		备用标志1     	X(1)       	*/
                //strClsExgSql.Append("B.Note          							   ");  /*12.		备注          	X(10)      	*/
                //strClsExgSql.Append("from            							   ");
                //strClsExgSql.Append("PRE_EMS3_DCR_HEAD A, PRE_EMS3_CLR_EXG B WHERE ");
                //strClsExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                //strClsExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                //strClsExgSql.Append(" AND A.DCR_TIMES = {DCR_TIMES}");
                //strClsExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                //strClsExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                //strClsExgSql.Append(" AND B.DCR_TIMES = {DCR_TIMES}");
                //strClsExgSql.Append(" ORDER BY B.G_NO,B.DU_CODE");

                //bErrCode = Ems3Data.readTableByAdapter(strClsExgSql.ToString(), QryExp, asaConn, out objRtnRult[5]);
                //if (bErrCode != 0)
                //{
                //    strErrMsg = "查询失败！";
                //    return bErrCode;
                //}
                #endregion

                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                  DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }
        public static int getNucDataOfMsg(//中期核查
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {//错误信息
            /*
             * 中期核查表头
             */
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);
            QryExp.Add("BEGIN_DATE", strOtherPara);

            strHeadSql.Append("select			");
            strHeadSql.Append("Ems_no       ,	");						   /*1.		电子帐册号  	X(12)	*/
            strHeadSql.Append("Trade_name   ,	");						   /*2.		经营单位名称	X(30)	*/
            strHeadSql.Append("Trade_code   ,	");						   /*3.		经营单位代码	X(10)	*/
            strHeadSql.Append("Muf_name     ,	");						   /*4.		加工单位名称	X(30)	*/
            strHeadSql.Append("Muf_code     ,	");						   /*5.		加工单位代码	X(10)	*/
            strHeadSql.Append("TO_CHAR(Begin_date,'YYYYMMDDHH24MMSS'),");  /*6.		本期起始日期	X(14)	*/
            strHeadSql.Append("TO_CHAR(Get_time,'YYYYMMDD'),");			   /*7.		海关提取时间	9(8) 	*/
            strHeadSql.Append("Get_Flag     ,	");						   /*8.		海关提取标志	X(1) 	*/
            strHeadSql.Append("Customs_Id       ");						   /*9.		主管海关代码	X(6) 	*/
            strHeadSql.Append("from             ");
            strHeadSql.Append("PRE_EMS3_COL_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");
            strHeadSql.Append(" AND BEGIN_DATE = {BEGIN_DATE}");

           // OracleConnection asaConn = DataManager.GetBizConn();
            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[3];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 中期核查成品
                 */
                StringBuilder strExgSql = new StringBuilder();
                strExgSql.Append("select			   ");
                strExgSql.Append("A.Ems_no         ,  ");								  /*1.		电子帐册号      	X(12)        */
                strExgSql.Append("TO_CHAR(B.G_no,'FM999999990'),");						  /*2.		序号            	Z(8)9        */
                strExgSql.Append("B.Cop_G_No       ,  ");								  /*3.		成品号/货号     	X(30)        */
                strExgSql.Append("B.Exg_version    ,  ");								  /*4.		成品版本        	X(15)        */
                strExgSql.Append("B.G_name         ,  ");								  /*5.		成品名称        	X(30)        */
                strExgSql.Append("B.Code_t_s       ,  ");								  /*6.		商品编号        	X(10)        */
                strExgSql.Append("B.G_model        ,  ");								  /*7.		规格型号        	X(30)        */
                strExgSql.Append("B.Unit           ,  ");								  /*8.		备案计量单位    	X(3)         */
                strExgSql.Append("TO_CHAR(NVL(B.G_s_amount,0),'" + N12_5Format + "'),");      /*9.		成品库存数量    	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.G_be_amount,0),'" + N12_5Format + "'),");     /*10.		成品在途数量    	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.G_out_n_amount,0),'" + N12_5Format + "'),");  /*11.		成品转出未报数量	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.G_in_amount,0),'" + N12_5Format + "'),");     /*12.		成品入库数量    	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.T_out_amount,0),'" + N12_5Format + "'),");    /*13.		本期成品出库数量	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.T_dom_amount,0),'" + N12_5Format + "'),");    /*14.		本期成品内销数量	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.T_aban_amount,0),'" + N12_5Format + "'),");   /*15.		本期成品放弃数量	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.G_change,0),'" + N12_5Format + "'),");        /*16.		成品退换        	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.Half_amount,0),'" + N12_5Format + "'),");     /*17.		半成品数量      	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.W_amount,0),'" + N12_5Format + "'),");        /*18.		废品数量        	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(NVL(B.Dis_amount,0),'" + N12_5Format + "'),");      /*19.		残次品数量      	Z(12)9.9(5)  */
                strExgSql.Append("TO_CHAR(B.Begin_date,'YYYYMMDDHH24MMSS')");              /*20.		本期起始日期    	X(14)        */
                strExgSql.Append(" from                ");
                strExgSql.Append("PRE_EMS3_COL_HEAD A,PRE_EMS3_COL_EXG B WHERE ");
                strExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" AND A.BEGIN_DATE = {BEGIN_DATE}");
                strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strExgSql.Append(" AND B.BEGIN_DATE = {BEGIN_DATE}");
                strExgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                /*
                 * 中期核查料件
                 */
                StringBuilder strImgSql = new StringBuilder();
                strImgSql.Append("select			   ");
                strImgSql.Append("A.ems_no          ,	");									/*1.		电子帐册号         	X(12)      	*/
                strImgSql.Append("TO_CHAR(B.G_no,'FM999999990'),");							/*2.		序号               	Z(8)9      	*/
                strImgSql.Append("B.cop_g_no        ,	");									/*3.		料号/货号          	X(30)      	*/
                strImgSql.Append("B.g_name          ,	");									/*4.		料件名称           	X(30)      	*/
                strImgSql.Append("B.code_T_S        ,	");									/*5.		商品编码           	X(10)      	*/
                strImgSql.Append("B.g_model         ,	");									/*6.		规格型号           	X(30)      	*/
                strImgSql.Append("B.unit            ,	");									/*7.		备案计量单位       	X(3)       	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_not_decl,0),'" + N12_5Format + "'),");		/*8.		转进未报数量       	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_on_way,0),'" + N12_5Format + "'),");		/*9.		原料在途数量       	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_stock,0),'" + N12_5Format + "'),");			/*10.		原料库存数量       	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_xr,0),'" + N12_5Format + "'),");			/*11.		废料数量           	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_online,0),'" + N12_5Format + "'),");		/*12.		在线数量           	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_gr,0),'" + N12_5Format + "'),");			/*13.		边角料数量         	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_enter_stock,0),'" + N12_5Format + "'),");  /*14.		本期原料入库数量   	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_rec,0),'" + N12_5Format + "'),");			/*15.		原料领料数量       	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.qty_sale_dem,0),'" + N12_5Format + "'),");		/*16.		本期原料内销数量   	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.consume_amt,0),'" + N12_5Format + "'),");		/*17.		本期放弃料件数量   	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.con_amount,0),'" + N12_5Format + "'),");		/*18.		合格成品耗用数量   	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.aban_amount,0),'" + N12_5Format + "'),");		/*19.		废品,残次品折料数量	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.img_giveup,0),'" + N12_5Format + "'),");		/*20.		本期放弃残次品折料 	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.img_change,0),'" + N12_5Format + "'),");		/*21.		原料退换           	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.half_g_amt,0),'" + N12_5Format + "'),");		/*22.		半成品折料数量     	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(NVL(B.img_again,0),'" + N12_5Format + "'),");			/*23.		原料复出           	Z(12)9.9(5)	*/
                strImgSql.Append("TO_CHAR(B.Begin_date,'YYYYMMDDHH24MMSS')");				/*24.		本期起始日期       	X(14)      	*/
                strImgSql.Append(" from                 ");
                strImgSql.Append("PRE_EMS3_COL_HEAD A,PRE_EMS3_COL_IMG B WHERE ");
                strImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO");
                strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE");
                strImgSql.Append(" AND A.BEGIN_DATE = {BEGIN_DATE");
                strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO");
                strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE");
                strImgSql.Append(" AND B.BEGIN_DATE = {BEGIN_DATE");
                strImgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                 // DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
                DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }

        /// <summary>
        /// 企业物料备案报文数据查询
        /// </summary>
        /// <param name="strSysFlg"></param>
        /// <param name="strMsgType"></param>
        /// <param name="strTradeCode"></param>
        /// <param name="strCopEmsNo"></param>
        /// <param name="strOtherPara"></param>
        /// <param name="objRtnRult"></param>
        /// <param name="strErrMsg"></param>
        /// <returns></returns>
        /// add by zhaoag 2011-11-07 
        public static int getEpzDataOfMsg(//企业物料备案
                                                                            string strSysFlg,//系统标志H88、H2000
                                                                            string strMsgType,//报文类型
                                                                            string strTradeCode,//企业十位编码
                                                                            string strCopEmsNo,//企业内部编号
                                                                            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
                                                                            ref DataTable[] objRtnRult, //返回结果
                                                                            ref string strErrMsg)
        {//错误信息
            /*
             * 企业物料备案表头
             */
            StringBuilder strHeadSql = new StringBuilder();
            Hashtable QryExp = new Hashtable();
            QryExp.Add("COP_EMS_NO", strCopEmsNo);
            QryExp.Add("TRADE_CODE", strTradeCode);

            //表头查询语句
            strHeadSql.Append("select                           ");
            strHeadSql.Append("'5001' Entry_Type                ,");		        //<Entry_Type>单证类型（固定值5001）</Entry_Type> 
            strHeadSql.Append("TRADE_CODE Ent_Code              ,");                //<Ent_Code>企业海关代码</Ent_Code>       
            strHeadSql.Append("COP_EMS_NO Cop_Ems_No            ,");		        //<Cop_Ems_No>企业内部编号</Cop_Ems_No>       
            strHeadSql.Append("EMS_NO Ems_No       		    ,");                //<Ems_No>账册编号</Ems_No>       
            strHeadSql.Append("I_E_PORT Customs_Code          ,");                  //<Customs_Code>关区代码</Customs_Code> 
            strHeadSql.Append("NOTE Note                      ,");                  //<Note>备注</Note>   
            strHeadSql.Append("'d' APPL_TYPE                  ,");                  //<APPL_TYPE>业务类型，d为企业物料、BOM表备案，c为进出区申请</APPL_TYPE> 
            strHeadSql.Append("DECLARE_TYPE DECLARE_TYPE      ,");                  //<DECLARE_TYPE>申报类型，1－新增， 2－变更，可空</DECLARE_TYPE> 
            strHeadSql.Append("INPUT_ER  Input_person           ");                 //<Input_person>录入人(海关监管系统账号)</Input_person> 
            strHeadSql.Append("FROM PRE_EMS3_HEAD WHERE ");
            strHeadSql.Append("COP_EMS_NO = {COP_EMS_NO}");
            strHeadSql.Append(" AND TRADE_CODE = {TRADE_CODE}");

            IDbConnection asaConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            objRtnRult = new DataTable[8];
            try
            {
                int bErrCode = Ems3Data.readTableByAdapter(strHeadSql.ToString(), QryExp, asaConn, out objRtnRult[0]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                if (1 == 2)//企业物料备案不需要查询这些数据 
                {
                    #region
                    /*
                 * 归并关系归并前成品
                 */
                    StringBuilder strOrgExgSql = new StringBuilder();

                    //成品查询语句 
                    strOrgExgSql.Append("select					");
                    strOrgExgSql.Append("B.COP_G_NO COP_G_NO                ,");                //<COP_G_NO>货号</COP_G_NO> 
                    strOrgExgSql.Append("B.G_NO G_NO       		            ,");                //<G_NO>归并后序号</G_NO> 
                    strOrgExgSql.Append("B.MODIFY_MARK MODIFY_MARK         	");                 //<MODIFY_MARK>修改标志</MODIFY_MARK> 								    
                    strOrgExgSql.Append("from                   ");
                    strOrgExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_EXG B  WHERE ");
                    strOrgExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                    strOrgExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                    strOrgExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                    strOrgExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                    strOrgExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                    strOrgExgSql.Append(" ORDER BY B.COP_G_NO");

                    bErrCode = Ems3Data.readTableByAdapter(strOrgExgSql.ToString(), QryExp, asaConn, out objRtnRult[1]);
                    if (bErrCode != 0)
                    {
                        strErrMsg = "查询失败！";
                        return bErrCode;
                    }

                    /*
                     * 归并关系归并前料件
                     */
                    StringBuilder strOrgImgSql = new StringBuilder();

                    //归并前料件查询语句
                    strOrgImgSql.Append("select					");
                    strOrgImgSql.Append("B.COP_G_NO COP_G_NO                ,");                //<COP_G_NO>货号</COP_G_NO> 
                    strOrgImgSql.Append("B.G_NO G_NO        		        ,");                //<G_NO>归并后序号</G_NO> 
                    strOrgImgSql.Append("B.MODIFY_MARK MODIFY_MARK        	");                 //<MODIFY_MARK>修改标志</MODIFY_MARK> 								    
                    strOrgImgSql.Append("from                   ");
                    strOrgImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_IMG B WHERE ");
                    strOrgImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                    strOrgImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                    strOrgImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                    strOrgImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                    strOrgImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                    strOrgImgSql.Append(" ORDER BY B.COP_G_NO");

                    bErrCode = Ems3Data.readTableByAdapter(strOrgImgSql.ToString(), QryExp, asaConn, out objRtnRult[2]);
                    if (bErrCode != 0)
                    {
                        strErrMsg = "查询失败！";
                        return bErrCode;
                    }

                    /*
                     * 归并关系归并后成品
                     */
                    StringBuilder strExgSql = new StringBuilder();

                    //归并后成品查询语句
                    strExgSql.Append("select					");
                    strExgSql.Append("B.G_NO G_NO        		    	,");		  //<G_NO>料件序号</G_NO>                        
                    strExgSql.Append("B.COP_G_NO COP_G_NO        		,");          //<COP_G_NO>料件货号</COP_G_NO>                
                    strExgSql.Append("substr(B.CODE_T_S,0,9) CODE_T    	,");          //<CODE_T>商品编码</CODE_T>                    
                    strExgSql.Append("substr(B.CODE_T_S,9) CODE_S      	,");          //<CODE_S>附加编码</CODE_S>                    
                    strExgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                    strExgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>商品规格型号</G_MODEL>                
                    strExgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                    strExgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                    strExgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                    strExgSql.Append("B.COUNTRY_CODE ORIGIN_COUNTRY 	,");          //<ORIGIN_COUNTRY>产销国(地区)</ORIGIN_COUNTRY> 
                    strExgSql.Append("B.DEC_PRICE UNIT_PRICE         	,");          //<UNIT_PRICE>企业申报单价</UNIT_PRICE>          
                    strExgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                    strExgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                    strExgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                    strExgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                    strExgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>            
                    strExgSql.Append("B.FACTOR_RATE FACTOR_RATE         ,");          //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>      
                    strExgSql.Append("B.QTY I_QTY         		        ,");          //<I_QTY>申报进口数量</I_QTY>                    
                    strExgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                    strExgSql.Append("B.FIRST_QTY ORIGIN_QTY         	,");          //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>          
                    strExgSql.Append("B.USE_TYPE USE_TYPE         		,");          //<USE_TYPE>用途代码</USE_TYPE>                
                    strExgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                    strExgSql.Append("B.NOTE_2 NOTE_2         		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                    strExgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>                          
                    strExgSql.Append("B.MODIFY_MARK MODIFY_MARK         ,");          //<MODIFY_MARK>修改标志</MODIFY_MARK>          
                    strExgSql.Append("'' APPR_AMT         		        ,");          //<APPR_AMT>总价</APPR_AMT>                  
                    strExgSql.Append("'' G_ENAME         		        ,");          //<G_ENAME>英文名称</G_ENAME>                  
                    strExgSql.Append("'' G_EMODEL         		        ,");          //<G_EMODEL>英文规格型号</G_EMODEL>              
                    strExgSql.Append("'' CLASS_NOTE         	        ,");          //<CLASS_NOTE>归类说明</CLASS_NOTE>            
                    strExgSql.Append("'' COP_UNIT         		        ,");          //<COP_UNIT>企业自编计量单位</COP_UNIT>            
                    strExgSql.Append("'' COP_FACTOR         	        ,");          //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>    
                    strExgSql.Append("'' DUTY_MODE         	            ");           //<DUTY_MODE>征免方式</DUTY_MODE>              
                    strExgSql.Append("from                       	    ");
                    strExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_EXG B WHERE ");
                    strExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                    strExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                    strExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                    strExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                    strExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                    strExgSql.Append(" ORDER BY B.G_NO");

                    bErrCode = Ems3Data.readTableByAdapter(strExgSql.ToString(), QryExp, asaConn, out objRtnRult[3]);
                    if (bErrCode != 0)
                    {
                        strErrMsg = "查询失败！";
                        return bErrCode;
                    }

                    /*
                     * 归并关系归并后料件
                     */
                    StringBuilder strImgSql = new StringBuilder();

                    //归并后料件查询语句
                    strImgSql.Append("select						    ");
                    strImgSql.Append("B.G_NO G_NO        		    	,");		  //<G_NO>料件序号</G_NO>                        
                    strImgSql.Append("B.COP_G_NO COP_G_NO        		,");          //<COP_G_NO>料件货号</COP_G_NO>                
                    strImgSql.Append("substr(B.CODE_T_S,0,9) CODE_T     ,");          //<CODE_T>商品编码</CODE_T>                    
                    strImgSql.Append("substr(B.CODE_T_S,9) CODE_S       ,");          //<CODE_S>附加编码</CODE_S>                    
                    strImgSql.Append("B.G_NAME G_NAME        			,");          //<G_NAME>商品名称</G_NAME>                    
                    strImgSql.Append("B.G_MODEL G_MODEL         		,");          //<G_MODEL>商品规格型号</G_MODEL>                
                    strImgSql.Append("B.UNIT UNIT         		    	,");          //<UNIT>申报计量单位</UNIT>                      
                    strImgSql.Append("B.UNIT_1 UNIT_1         			,");          //<UNIT_1>法定计量单位</UNIT_1>                  
                    strImgSql.Append("B.UNIT_2 UNIT_2         			,");          //<UNIT_2>法定第二单位</UNIT_2>                  
                    strImgSql.Append("B.COUNTRY_CODE ORIGIN_COUNTRY 	,");          //<ORIGIN_COUNTRY>产销国(地区)</ORIGIN_COUNTRY> 
                    strImgSql.Append("B.DEC_PRICE UNIT_PRICE         	,");          //<UNIT_PRICE>企业申报单价</UNIT_PRICE>          
                    strImgSql.Append("B.CURR CURR         		    	,");          //<CURR>币制</CURR>                          
                    strImgSql.Append("B.DEC_PRICE_RMB DEC_PRICE_RMB     ,");          //<DEC_PRICE_RMB>申报单价人民币</DEC_PRICE_RMB>   
                    strImgSql.Append("B.FACTOR_1 FACTOR_1         		,");          //<FACTOR_1>法定计量单位比例因子</FACTOR_1>          
                    strImgSql.Append("B.FACTOR_2 FACTOR_2         		,");          //<FACTOR_2>第二法定计量单位比例因子</FACTOR_2>        
                    strImgSql.Append("B.FACTOR_WT FACTOR_WT         	,");          //<FACTOR_WT>重量比例因子</FACTOR_WT>            
                    strImgSql.Append("B.FACTOR_RATE FACTOR_RATE         ,");          //<FACTOR_RATE>比例因子浮动比率</FACTOR_RATE>      
                    strImgSql.Append("B.QTY I_QTY         		        ,");          //<I_QTY>申报进口数量</I_QTY>                    
                    strImgSql.Append("B.MAX_QTY MAX_QTY         		,");          //<MAX_QTY>批准最大余量</MAX_QTY>                
                    strImgSql.Append("B.FIRST_QTY ORIGIN_QTY         	,");          //<ORIGIN_QTY>初始库存数量</ORIGIN_QTY>          
                    strImgSql.Append("B.USE_TYPE USE_TYPE         		,");          //<USE_TYPE>用途代码</USE_TYPE>                
                    strImgSql.Append("B.NOTE_1 NOTE_1         		    ,");          //<NOTE_1>备用标志1</NOTE_1>                   
                    strImgSql.Append("B.NOTE_2 NOTE_2         		    ,");          //<NOTE_2>备用标志2</NOTE_2>                   
                    strImgSql.Append("B.NOTE NOTE	         		    ,");          //<NOTE>备注</NOTE>                          
                    strImgSql.Append("B.MODIFY_MARK MODIFY_MARK         ,");          //<MODIFY_MARK>修改标志</MODIFY_MARK>          
                    strImgSql.Append("'' APPR_AMT         		,");          //<APPR_AMT>总价</APPR_AMT>                  
                    strImgSql.Append("'' G_ENAME         		,");          //<G_ENAME>英文名称</G_ENAME>                  
                    strImgSql.Append("'' G_EMODEL         		,");          //<G_EMODEL>英文规格型号</G_EMODEL>              
                    strImgSql.Append("'' CLASS_NOTE         	,");          //<CLASS_NOTE>归类说明</CLASS_NOTE>            
                    strImgSql.Append("'' COP_UNIT         		,");          //<COP_UNIT>企业自编计量单位</COP_UNIT>            
                    strImgSql.Append("'' COP_FACTOR         	,");          //<COP_FACTOR>企业自编计量单位比例因子</COP_FACTOR>    
                    strImgSql.Append("'' DUTY_MODE         	");           //<DUTY_MODE>征免方式</DUTY_MODE>              
                    strImgSql.Append("from                       	    ");
                    strImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_IMG B WHERE ");
                    strImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                    strImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                    strImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                    strImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                    strImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                    strImgSql.Append(" ORDER BY B.G_NO");

                    bErrCode = Ems3Data.readTableByAdapter(strImgSql.ToString(), QryExp, asaConn, out objRtnRult[4]);
                    if (bErrCode != 0)
                    {
                        strErrMsg = "查询失败！";
                        return bErrCode;
                    }
                    #endregion
                }
                /*
                 * 归并关系BOM
                 */
                StringBuilder strBomSql = new StringBuilder();

                //BOM查询语句 
                strBomSql.Append("select				        ");
                strBomSql.Append("''            entry_id        ,");	  //<entry_id>留空</entry_id>               						
                strBomSql.Append("COP_EXG_NO    Pro_MaterielID  ,");	  //<Pro_MaterielID>企业成品编号</Pro_MaterielID> 					
                strBomSql.Append("B.COP_IMG_NO  Materiel_ID     ,");	  //<Materiel_ID>企业物料编号</Materiel_ID>               					
                strBomSql.Append("B.DEC_CM      Unit_Ratio      ,");	  //<Unit_Ratio>单耗</Unit_Ratio>                         					
                strBomSql.Append("B.DEC_DM      Scr_Ratio       ,");	  //<Scr_Ratio>损耗率</Scr_Ratio>                        					
                strBomSql.Append("B.BEGIN_DATE  Version         ,");	  //<Version>版本号</Version>             					
                strBomSql.Append("''            Begin_Date      ,");	  //<Begin_Date>开始有效日期</Begin_Date>        
                strBomSql.Append("''            End_Date        ,");	  //<End_Date>结束有效日期</End_Date>     	
                strBomSql.Append("B.NOTE        Note            ");	      //<Note>备注</Note>                             					
                strBomSql.Append("from                 ");
                strBomSql.Append("PRE_EMS3_HEAD A, PRE_EMS3_ORG_BOM B WHERE ");
                strBomSql.Append(" B.MODIFY_MARK!='0' AND ");
                strBomSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strBomSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strBomSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strBomSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strBomSql.Append(" ORDER BY B.COP_EXG_NO,B.BEGIN_DATE,B.COP_IMG_NO");

                bErrCode = Ems3Data.readTableByAdapter(strBomSql.ToString(), QryExp, asaConn, out objRtnRult[5]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                //归并前数据报文中的归并前料件
                StringBuilder strMrOrgImgSql = new StringBuilder();
                strMrOrgImgSql.Append("select					    ");
                strMrOrgImgSql.Append("B.COP_G_NO     MaterielID  ,");      //<MaterielID>企业物料编号</MaterielID>                         
                strMrOrgImgSql.Append("B.G_NO           Ent_GNo     ,");      //<Ent_GNo>企业物料账册序号</Ent_GNo>                              
                strMrOrgImgSql.Append("B.G_NAME         Ent_GName   ,");      //<Ent_GName>料件名称</Ent_GName>                              
                strMrOrgImgSql.Append("B.G_MODEL        Ent_GModel  ,");      //<Ent_GModel>料件规格型号</Ent_GModel>                          
                strMrOrgImgSql.Append("'1'              Ent_GMark   ,");      //<Ent_GMark>料件类型，1为料件，3为成品</Ent_GMark>                                
                strMrOrgImgSql.Append("B.DEC_PRICE      Price       ,");      //<Price>价格</Price>   
                strMrOrgImgSql.Append("B.UNIT           Unit        ,");      //<Unit>计量单位</Unit>   
                strMrOrgImgSql.Append("'1'              Main_Flag   ,");      //<Main_Flag>固定值1</Main_Flag>     
                strMrOrgImgSql.Append("'01'             Shap_Code   ,");      //<Shap_Code>固定值01</Shap_Code>  
                strMrOrgImgSql.Append("B.NOTE           Note        ");      //<Note>备注</Note>    
                strMrOrgImgSql.Append("from                   ");
                strMrOrgImgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_IMG B WHERE ");
                strMrOrgImgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strMrOrgImgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgImgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strMrOrgImgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgImgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strMrOrgImgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strMrOrgImgSql.ToString(), QryExp, asaConn, out objRtnRult[6]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }

                //归并前数据报文中的归并前成品
                StringBuilder strMrOrgExgSql = new StringBuilder();
                strMrOrgExgSql.Append("select					     ");
                strMrOrgExgSql.Append("B.COP_G_NO     MaterielID  ,");      //<MaterielID>企业物料编号</MaterielID>                         
                strMrOrgExgSql.Append("B.G_NO           Ent_GNo     ,");      //<Ent_GNo>企业物料账册序号</Ent_GNo>                              
                strMrOrgExgSql.Append("B.G_NAME         Ent_GName   ,");      //<Ent_GName>料件名称</Ent_GName>                              
                strMrOrgExgSql.Append("B.G_MODEL        Ent_GModel  ,");      //<Ent_GModel>料件规格型号</Ent_GModel>                          
                strMrOrgExgSql.Append("'3'              Ent_GMark   ,");      //<Ent_GMark>料件类型，1为料件，3为成品</Ent_GMark>                                
                strMrOrgExgSql.Append("B.DEC_PRICE      Price       ,");      //<Price>价格</Price>   
                strMrOrgExgSql.Append("B.UNIT           Unit        ,");      //<Unit>计量单位</Unit>   
                strMrOrgExgSql.Append("'1'              Main_Flag   ,");      //<Main_Flag>固定值1</Main_Flag>     
                strMrOrgExgSql.Append("'01'             Shap_Code   ,");      //<Shap_Code>固定值01</Shap_Code>  
                strMrOrgExgSql.Append("B.NOTE           Note         ");      //<Note>备注</Note>                       
                strMrOrgExgSql.Append("from                   ");
                strMrOrgExgSql.Append("PRE_EMS3_HEAD A,PRE_EMS3_ORG_EXG B WHERE ");
                strMrOrgExgSql.Append(" B.MODIFY_MARK!='0' AND ");
                strMrOrgExgSql.Append("A.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgExgSql.Append(" AND A.TRADE_CODE = {TRADE_CODE}");
                strMrOrgExgSql.Append(" AND B.COP_EMS_NO = {COP_EMS_NO}");
                strMrOrgExgSql.Append(" AND B.TRADE_CODE = {TRADE_CODE}");
                strMrOrgExgSql.Append(" ORDER BY B.COP_G_NO");

                bErrCode = Ems3Data.readTableByAdapter(strMrOrgExgSql.ToString(), QryExp, asaConn, out objRtnRult[7]);
                if (bErrCode != 0)
                {
                    strErrMsg = "查询失败！";
                    return bErrCode;
                }


                strErrMsg = "查询成功！";
                return 0;
            }
            finally
            {
                //   DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
                DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, asaConn);
            }
        }
        

        /// <summary>
        /// 将报文写入数据库
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="strMsg"></param>
        /// <param name="strErrMsg"></param>
        /// <returns></returns>
        public static int WriteMsgFileToDB(string strMsgType, string strFileName, StringBuilder strMsg, IDbTransaction m_Trans, ref string strErrMsg)
        {
            strErrMsg = string.Empty;
            //OracleConnection m_DbConn = DataManager.GetBizConn();
            //OracleCommand m_DbCommand = new OracleCommand();
            IDbCommand dbCommand = DataMgr.Instance.CreateCommand(Ems3Dict.strEms, m_Trans);
            //OracleTransaction m_Trans = m_DbConn.BeginTransaction();
            //m_DbCommand.BindByName = true;
            dbCommand.Connection = m_Trans.Connection;

            string strSql = "INSERT INTO EMS3_MSG_RECORD(FILE_NAME,BUILD_DATE,SEND_FLAG,MSG_TYPE,FILE_INFO)VALUES({FILE_NAME},GETDATE(),{SEND_FLAG},{MSG_TYPE},{FILE_INFO})";
            try
            {
                strMsgType = strMsgType.Substring(0, 6);
                dbCommand.CommandText = strSql;
                dbCommand.Parameters.Add(DataMgr.Instance.CreateDataParameter(Ems3Dict.strEms, "FILE_NAME", strFileName));
                dbCommand.Parameters.Add(DataMgr.Instance.CreateDataParameter(Ems3Dict.strEms, "SEND_FLAG", "0"));
                dbCommand.Parameters.Add(DataMgr.Instance.CreateDataParameter(Ems3Dict.strEms, "MSG_TYPE", strMsgType));
                dbCommand.Parameters.Add(DataMgr.Instance.CreateDataParameter(Ems3Dict.strEms, "FILE_INFO", strMsg.ToString()));
                //m_DbCommand.Parameters.Add("FILE_NAME", OracleDbType.Varchar2, strFileName, ParameterDirection.Input);
                //m_DbCommand.Parameters.Add("SEND_FLAG", OracleDbType.Varchar2, "0", ParameterDirection.Input);
                //m_DbCommand.Parameters.Add("MSG_TYPE", OracleDbType.Varchar2, strMsgType, ParameterDirection.Input);
                //System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(936);
                //m_DbCommand.Parameters.Add("FILE_INFO", OracleDbType.Clob, strMsg.ToString(), ParameterDirection.Input);
                dbCommand.ExecuteNonQuery();
                //				m_Trans.Commit();
                return 0;
            }
            catch (Exception e)
            {
                strErrMsg = "报文入库失败：" + e.Message;
                LogMgr.WriteInfo(Ems3Dict.strEms, "电子帐册报文入库失败，SQL语句：" + dbCommand.CommandText);
                LogMgr.WriteInfo(Ems3Dict.strEms, "电子帐册报文入库失败，错误原因：" + strErrMsg);
                LogMgr.WriteInfo(Ems3Dict.strEms, "电子帐册报文入库失败，文件名：" + strFileName);
                LogMgr.WriteInfo(Ems3Dict.strEms, "电子帐册报文入库失败，报文类型：" + strMsgType);
                LogMgr.WriteInfo(Ems3Dict.strEms, "电子帐册报文入库失败，报文内容：\n" + strMsg);
                //				m_Trans.Rollback();
                return -1;
            }
            //			finally
            //			{
            //				DataManager.CloseConn(m_DbConn);
            //			}
        }

        /// <summary>
        /// 保存报文及更新状态表
        /// </summary>
        /// <param name="strMsgType"></param>
        /// <param name="strTradeCode"></param>
        /// <param name="strCopEmsNo"></param>
        /// <param name="strOtherPara"></param>
        /// <param name="strFileName"></param>
        /// <param name="strCurDate"></param>
        /// <param name="strHostId"></param>
        /// <param name="strMsg"></param>
        /// <param name="strOtherMsg"></param>
        /// <param name="strErrMsg"></param>
        /// <returns></returns>
        public static int SaveMsgAndUpdateStat(string strMsgType,//报文类型
            string strTradeCode,//企业十位编码
            string strCopEmsNo,//企业内部编号
            string strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次数
            string strFileName,//报文名
            string strCurDate,//申报时间
            string strHostId,//hostId
            StringBuilder strMsg,//第一个报文
            StringBuilder strOtherMsg,//第二个报文（仅归并关系使用）
            ref string strErrMsg)
        {
            int bErrCode = 0;
            strErrMsg = string.Empty;
         //   OracleConnection m_DbConn = DataManager.GetBizConn();
          //  OracleTransaction m_Trans = m_DbConn.BeginTransaction();
            IDbConnection m_DbConn = DataMgr.Instance.CreateConnection(Ems3Dict.strEms);
            m_DbConn.Open();
            IDbTransaction m_Trans = m_DbConn.BeginTransaction();

            try
            {
                //更新状态表
                bErrCode = Ems3SvrBuildMsgData.updateStat(strMsgType,//报文类型
                    strTradeCode,//企业十位编码
                    strCopEmsNo,//企业内部编号
                    strOtherPara,//其他差数,如:中期和查的开始日期，报核的报核次
                    m_Trans,
                    ref strErrMsg);
                LogMgr.WriteWarning(Ems3Dict.strEms, "LSK1" + bErrCode.ToString());
                LogMgr.WriteWarning(Ems3Dict.strEms, "LSK1" + strTradeCode.ToString());
                LogMgr.WriteWarning(Ems3Dict.strEms, "LSK1" + strCopEmsNo.ToString());
                LogMgr.WriteWarning(Ems3Dict.strEms, "LSK1" + strOtherPara.ToString());
                LogMgr.WriteWarning(Ems3Dict.strEms, "LSK1" + strErrMsg.ToString());

                if (bErrCode == -1)
                {
                    m_Trans.Rollback();
                    return bErrCode;
                }

                //报文不用入库 comment by ccx 2010-5-13
                ////以下MQ迁移才增加
                ////---生成文件1---
                //bErrCode = Ems3SvrBuildMsgData.WriteMsgFileToDB(strMsgType, strFileName, strMsg, m_Trans, ref strErrMsg);
                //if (bErrCode == -1)
                //{
                //    m_Trans.Rollback();
                //    return bErrCode;
                //}
                ////归并关系写第二个报文
                //if (strMsgType == "EMS212" || strMsgType == "EMS222")
                //{
                //    string strMsgType2 = string.Empty;
                //    if (strMsgType == "EMS212") strMsgType2 = "EMS215";
                //    if (strMsgType == "EMS222") strMsgType2 = "EMS225";

                //    strFileName = strMsgType2 + "_" + strCurDate + "_" + strTradeCode + "_" + strHostId + ".EMS";
                //    bErrCode = Ems3SvrBuildMsgData.WriteMsgFileToDB(strMsgType, strFileName, strOtherMsg, m_Trans, ref strErrMsg);
                //    if (bErrCode == -1)
                //    {
                //        m_Trans.Rollback();
                //        return bErrCode;
                //    }
                //}

                m_Trans.Commit();
                return bErrCode;
            }
            catch (Exception e)
            {
                strErrMsg = "报文入库失败：" + e.Message;
                m_Trans.Rollback();
                return -1;
            }
            finally
            {
                DataMgr.Instance.ReleaseConnection(Ems3Dict.strEms, m_DbConn);
              //  DataManager.CloseConn(m_DbConn);
            }
        }

    }
}
